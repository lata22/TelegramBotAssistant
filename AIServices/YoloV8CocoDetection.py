from ultralytics import YOLO

# Load a model
model = YOLO('yolov8x.pt')  # load an official model

# Predict with the model
results = model('https://ultralytics.com/images/bus.jpg')  # predict on an image
for i, result in enumerate(results):
    boxes = result.boxes  # Boxes object for bbox outputs
    classes = boxes.cls  # Classes of the detections
    probs = result.probs  # Class probabilities for classification outputs
    print(f'Result #{i+1}:')
    print(f'Bounding boxes: {boxes}')
    print(f'Detected classes: {classes}')
    print(f'Class probabilities: {probs}')
    print("\n")