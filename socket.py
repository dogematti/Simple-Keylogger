import socket
import threading
import argparse
import logging

# Configure logging to display timestamps and log levels
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

def handle_client(client_socket):
    try:
        data = client_socket.recv(1024).decode()
        if data:
            logging.info(f"Received data: {data}")
    except Exception as e:
        logging.error(f"Client handling error: {e}")
    finally:
        client_socket.close()

def start_server(host, port):
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
            server_socket.bind((host, port))
            server_socket.listen()
            logging.info(f"Server listening on {host}:{port}")

            while True:
                client_socket, client_address = server_socket.accept()
                logging.info(f"Accepted connection from {client_address[0]}:{client_address[1]}")
                client_thread = threading.Thread(target=handle_client, args=(client_socket,))
                client_thread.start()
    except Exception as e:
        logging.error(f"Server error: {e}")

def parse_args():
    parser = argparse.ArgumentParser(description="Multithreaded server with IP and port options")
    parser.add_argument("-ip", "--host", default='127.0.0.1', help="Server IP address")
    parser.add_argument("-p", "--port", type=int, default=12345, help="Server port number")
    return parser.parse_args()

if __name__ == "__main__":
    args = parse_args()
    HOST = args.host
    PORT = args.port
    start_server(HOST, PORT)
