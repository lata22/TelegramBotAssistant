# Telegram Bot Assistant

## Table of Contents

- [Introduction](#introduction)
- [Technologies](#technologies)
- [Structure](#structure)
    - [AIServices](#aiservices)
    - [CognitiveServices](#cognitiveservices)
    - [VD.TelegramBot](#vdtelegrambot)

## Introduction

This project aims to provide a comprehensive Telegram Bot Assistant, capable of various tasks ranging from AI operations, cognitive services to chat handling and more.

## Technologies

- Python
- C#
- SQL
- Postgres
- AWS
- Azure

## Structure

The project is divided into multiple components each serving a specific purpose.

### AIServices

#### AIService.py
- Core AI functionalities

#### AIServiceInVirtualEnv.py
- Virtual environment specific AI functionalities

#### FaceDetectionImageHandler.py
- Handles face detection in images

#### FaceDetectionVideoHandler.py
- Handles face detection in videos

#### SDImageCreationService.py
- Service for creating SD images

#### YoloV8CocoDetection.py
- Object detection using YoloV8 and Coco data set

### CognitiveServices

#### CognitiveServices.sln
- Solution file for all cognitive services

#### Program.cs
- Main entry point for the API

[Click here for full CognitiveServices Structure](CognitiveServices.md)

### VD.TelegramBot

#### TelegramBotHostedService.cs
- Hosted service for Telegram bot

#### TelegramBotService.cs
- Core functionalities of Telegram bot

[Click here for full VD.TelegramBot Structure](VD.TelegramBot.md)
