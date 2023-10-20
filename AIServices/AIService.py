import asyncio
import aio_pika
import torch
from diffusers import DiffusionPipeline
from FaceDetectionImageHandler import process_image_messages
from FaceDetectionVideoHandler import process_video_messages
from SDImageCreationService import process_image_creation_messages
import redis

async def main():

    print(f'Cuda Version: {torch.version.cuda}')
    print(f'Is cuda available: {torch.cuda.is_available()}')
    device = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')

    # Create a Redis client
    redis_connection = redis.Redis(host='ip', port=6379, password="password")
    personRecognitionModel = torch.hub.load('ultralytics/yolov5', 'custom', path='best.pt', force_reload=True, device=device)

    # load both base & refiner
    SDBase = DiffusionPipeline.from_pretrained(
        "stabilityai/stable-diffusion-xl-base-1.0", torch_dtype=torch.float16, variant="fp16", use_safetensors=True)
    SDBase.to(device)

    SDRefiner = DiffusionPipeline.from_pretrained(
        "stabilityai/stable-diffusion-xl-refiner-1.0",
        text_encoder_2=SDBase.text_encoder_2,
        vae=SDBase.vae,
        torch_dtype=torch.float16,
        use_safetensors=True,
        variant="fp16",)
    SDRefiner.to(device)

    connection = await aio_pika.connect_robust('amqp://user:password@ip:5672/')
    channel = await connection.channel()

    # Code to handle FaceDetectionCreated messages
    face_detection_queue = await channel.declare_queue('FaceDetectionRequested_Queue', durable=True)
    face_detection_exchange = await channel.declare_exchange('Infrastructure.Messaging.Events:FaceDetectionRequested', type='fanout', durable=True)
    await face_detection_queue.bind(face_detection_exchange, routing_key='FaceDetectionRequested')

    # Code to handle FaceDetectionVideoCreated messages
    face_detection_video_queue = await channel.declare_queue('FaceDetectionVideoRequested_Queue', durable=True)
    face_detection_video_exchange = await channel.declare_exchange('Infrastructure.Messaging.Events:FaceDetectionVideoRequested', type='fanout',durable=True)
    await face_detection_video_queue.bind(face_detection_video_exchange, routing_key='FaceDetectionVideoRequested')

    # Code to handle FaceDetectionVideoCreated messages
    image_creation_queue = await channel.declare_queue('SDImageCreationRequested_Queue', durable=True)
    image_creation_exchange = await channel.declare_exchange('Infrastructure.Messaging.Events:SDImageCreationRequested', type='fanout',durable=True)
    await image_creation_queue.bind(image_creation_exchange, routing_key='SDImageCreationRequested')

    await asyncio.gather(
        process_image_messages(face_detection_queue, personRecognitionModel, connection, redis_connection), 
        process_video_messages(face_detection_video_queue, personRecognitionModel, connection, redis_connection),
        process_image_creation_messages(image_creation_queue, SDBase, SDRefiner, connection, redis_connection))

if __name__ == "__main__":
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())



