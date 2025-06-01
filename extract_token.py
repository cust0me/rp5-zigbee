import os
import time

token_file = '/init/token.txt'
env_file = '/work/.env'
timeout = 1  # seconds
interval = 1  # seconds

# Wait for token.txt to exist
start = time.time()
while not os.path.exists(token_file):
  if time.time() - start > timeout:
    print('Timeout waiting for token.txt')
    exit(1)
  time.sleep(interval)

with open(token_file) as f:
  lines = f.readlines()
  if len(lines) > 1:
    parts = lines[1].split()
    if len(parts) >= 3:
      token = parts[2]
      new_line = f'INFLUXDB_TOKEN={token}\n'
      try:
        with open(env_file, 'r') as env:
          env_lines = env.readlines()
      except FileNotFoundError:
        env_lines = []

      found = False
      for i, line in enumerate(env_lines):
        if line.startswith('INFLUXDB_TOKEN='):
          env_lines[i] = new_line
          found = True
          break
      if not found:
        if env_lines and not env_lines[-1].endswith('\n'):
          env_lines[-1] += '\n'
        env_lines.append(new_line)

      with open(env_file, 'w') as env:
        env.writelines(env_lines)
      print('Token extracted and written to .env')
    else:
      print('Token line does not have enough columns')
  else:
    print('Token not found in token.txt')