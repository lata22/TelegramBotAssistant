import cv2
import numpy as np
import json
import aio_pika
import uuid

async def process_image_creation_messages(image_creation_queue, base, refiner, connection, redis_connection):
    async with image_creation_queue.iterator() as image_creation_iter:
        async for image_creation_message in image_creation_iter:
            await handle_image_creation_messages(image_creation_message, base, refiner, connection, redis_connection)


async def handle_image_creation_messages(message, base, refiner, connection, redis_connection):
    print('SDImageCreationRequested message arrived')
    async with message.process():
        body = message.body.decode('utf-8')
        event = json.loads(body)
        prompt = event["message"]["prompt"]
        chatId = event["message"]["chatId"]

        # Define how many steps and what % of steps to be run on each experts (80/20) here
        n_steps = 40
        high_noise_frac = 0.8

        print('Running in StableDiffusion-XL model')
        # run both experts
        image = base(
            prompt=prompt,
            num_inference_steps=n_steps,
            denoising_end=high_noise_frac,
            output_type="latent",
        ).images
        image = refiner(
            prompt=prompt,
            num_inference_steps=n_steps,
            denoising_start=high_noise_frac,
            image=image,
        ).images
        print('Starting to produce Event SDImageCreationProcessed')
        # Produce the event with the processed video
        image_np = np.array(image[0])
        image_np = cv2.cvtColor(image_np, cv2.COLOR_BGR2RGB)
        await produce_image_creation_processed_event(image_np, chatId, connection, redis_connection)



async def produce_image_creation_processed_event(image, chatId, connection, redis_connection):
    # Convert the image to bytes
    _, image_bytes = cv2.imencode('.jpg', image, (cv2.IMWRITE_JPEG_QUALITY, 100))
    if isinstance(image_bytes, np.ndarray):
        image_bytes = image_bytes.tobytes()
    # Generate a GUID for the cache_key
    cache_key = str(uuid.uuid4())
    # Store the image into Redis cache
    redis_connection.set(cache_key, image_bytes)

    channel = await connection.channel()
    exchange = await channel.get_exchange(
        'Infrastructure.Messaging.Events:SDImageCreationProcessed',
        ensure=True)
    
    # Declare queue
    queue = await channel.get_queue('SDImageCreationProcessedMessageHandler')
    # Bind queue to exchange
    await queue.bind(exchange, routing_key='')
    message = json.dumps({ 
        "destinationAddress":"rabbitmq://192.168.1.136/Infrastructure.Messaging.Events:SDImageCreationProcessed", 
        "messageType": [ "urn:message:Infrastructure.Messaging.Events:SDImageCreationProcessed"],
        "message": { "CacheKey": cache_key , "ChatId": chatId}}).encode('utf-8')
    await exchange.publish(aio_pika.Message(body=message, content_type='application/vnd.masstransit+json'), routing_key='')
    print('Event SDImageCreationProcessed rised')