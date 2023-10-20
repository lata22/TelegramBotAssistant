import uuid
import aio_pika
import cv2
import numpy as np
import json

async def process_image_messages(queue, personRecognitionModel, connection, redis_connection):
    async with queue.iterator() as queue_iter:
        async for message in queue_iter:
            await handle_message(message, personRecognitionModel, connection, redis_connection)

async def handle_message(message, personRecognitionModel, connection, redis_connection):
    print('FaceDetectionRequested message arrived')
    async with message.process():
        body = message.body.decode('utf-8')
        event = json.loads(body)
        cache_key = event["message"]["cacheKey"]
        chatId =  event["message"]["chatId"]
        img_bytes = redis_connection.get(cache_key)
        nparr = np.frombuffer(img_bytes, np.uint8)
        image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        results = personRecognitionModel(image)  
        encodeResult, buffer = cv2.imencode('.jpg', results.render()[0], (cv2.IMWRITE_JPEG_QUALITY, 100))
        if encodeResult:
            processed_cache_key = str(uuid.uuid4())
            redis_connection.set(processed_cache_key, buffer.tobytes())
            await produce_face_detection_processed_event(processed_cache_key, chatId, connection)



async def produce_face_detection_processed_event(processed_cache_key, chatId, connection):
    channel = await connection.channel()
    exchange = await channel.get_exchange(
        'Infrastructure.Messaging.Events:FaceDetectionProcessed',
        ensure=True)
    
    # Declare queue
    queue = await channel.get_queue('FaceDetectionProcessedMessageHandler')
    
    # Bind queue to exchange
    await queue.bind(exchange, routing_key='')

    message = json.dumps({ 
        "destinationAddress":"rabbitmq://192.168.1.136/Infrastructure.Messaging.Events:FaceDetectionProcessed", 
        "messageType": [ "urn:message:Infrastructure.Messaging.Events:FaceDetectionProcessed"],
        "message": { "CacheKey": processed_cache_key , "ChatId": chatId}}).encode('utf-8')
    await exchange.publish(aio_pika.Message(body=message, content_type='application/vnd.masstransit+json'), routing_key='')
    print('Event FaceDetectionProcessed rised')

