import venv
import subprocess
import sys
import os

env_dir = "virtualenv"

# Determine the Python executable inside the virtual environment
venv_python = f"{env_dir}/bin/python" if sys.platform != 'win32' else f"{env_dir}\\Scripts\\python.exe"

if not os.path.exists(env_dir):
    # Create the virtual environment
    builder = venv.EnvBuilder(with_pip=True)
    builder.create(env_dir)
    # Install dependencies from the requirements.txt file
    subprocess.run([venv_python, "-m", "pip", "install", "-r", "requirements.txt"])

# Continue with the rest of your code
subprocess.run([venv_python, "AIService.py"])