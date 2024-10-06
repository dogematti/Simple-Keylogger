import socket

# Server IP and port
server_ip = "0.0.0.0"  # Listen on all interfaces
server_port = 5555

# Create the server socket
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((server_ip, server_port))
server_socket.listen(5)

print(f"Listening for connections on {server_ip}:{server_port}...")

while True:
    client_socket, addr = server_socket.accept()
    print(f"Connection from {addr}")

    data = client_socket.recv(1024).decode('utf-8')
    if data:
        print(f"Received: {data}")
        # Reply with "ok"
        client_socket.send("ok".encode('utf-8'))

    client_socket.close()