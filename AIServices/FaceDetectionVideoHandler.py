import uuid
import aio_pika
import cv2
import numpy as np
import json
import av
import io


async def process_video_messages(video_queue, personRecognitionModel, connection, redis_connection):
    async with video_queue.iterator() as video_queue_iter:
        async for video_message in video_queue_iter:
            await handle_video_message(video_message, personRecognitionModel, connection, redis_connection)


async def handle_video_message(message, personRecognitionModel, connection, redis_connection):
    print('FaceDetectionVideoRequested message arrived')
    async with message.process():
        body = message.body.decode('utf-8')
        event = json.loads(body)
        cache_key = event["message"]["cacheKey"]
        chatId = event["message"]["chatId"]
        video_bytes = redis_connection.get(cache_key)
        
        # Initialize the input container
        input_container = av.open(io.BytesIO(video_bytes))
        
        # Get the input video stream
        input_video_stream = next(s for s in input_container.streams if s.type == 'video')
        fps = input_video_stream.average_rate
        
        # Create an output container
        output_buffer = io.BytesIO()
        output_container = av.open(output_buffer, mode='w', format='mp4')
        
        # Add an output video stream
        output_video_stream = output_container.add_stream('libx264', fps)
        output_video_stream.width = input_video_stream.width
        output_video_stream.height = input_video_stream.height
        output_video_stream.options = {
            'crf': '20',  # Constant Rate Factor: Lower values mean better quality, higher values mean smaller file size
            'preset': 'medium'  # Encoding speed/quality trade-off: medium is a good balance
        }

        # Process each frame
        for frame in input_container.decode(input_video_stream):
            # Convert the frame from PyAV to OpenCV
            image = cv2.cvtColor(np.array(frame.to_image()), cv2.COLOR_RGB2BGR)
            
            # Process the image with personRecognitionModel
            results = personRecognitionModel(image)
            processed_image = results.render()[0]
            
            # Convert the processed image from OpenCV to PyAV
            processed_frame = av.VideoFrame.from_ndarray(processed_image, format='bgr24')
            
            # Encode the processed frame
            for packet in output_video_stream.encode(processed_frame):
                output_container.mux(packet)
        
        # Flush the output container
        for packet in output_video_stream.encode():
            output_container.mux(packet)
        
        # Close the input and output containers
        input_container.close()
        output_container.close()
        
        # Get the output video bytes and encode to base64
        output_video_bytes = output_buffer.getvalue()
        processed_video_cache_key = str(uuid.uuid4())
        redis_connection.set(processed_video_cache_key, output_video_bytes)
        
        # Produce the event with the processed video
        await produce_face_detection_video_processed_event(processed_video_cache_key, chatId, connection)


async def produce_face_detection_video_processed_event(processed_video_cache_key, chatId, connection):
    channel = await connection.channel()
    exchange = await channel.get_exchange(
        'Infrastructure.Messaging.Events:FaceDetectionVideoProcessed',
        ensure=True)
    
    # Declare queue
    queue = await channel.get_queue('FaceDetectionVideoProcessedMessageHandler')
    # Bind queue to exchange
    await queue.bind(exchange, routing_key='')

    message = json.dumps({ 
        "destinationAddress":"rabbitmq://192.168.1.136/Infrastructure.Messaging.Events:FaceDetectionVideoProcessed", 
        "messageType": [ "urn:message:Infrastructure.Messaging.Events:FaceDetectionVideoProcessed"],
        "message": { "CacheKey": processed_video_cache_key , "ChatId": chatId}}).encode('utf-8')
    await exchange.publish(aio_pika.Message(body=message, content_type='application/vnd.masstransit+json'), routing_key='')
    print('Event FaceDetectionVideoProcessed rised')






