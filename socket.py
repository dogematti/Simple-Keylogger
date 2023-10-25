import socket
import threading
import argparse  # Import the argparse module for command-line arguments

# Function to handle an individual client connection
def handle_client(client_socket):
    try:
        # Receive data from the client
        data = client_socket.recv(1024).decode()
        # Process the received data (for analysis or further processing)
        print(f"Received data: {data}")
    except Exception as e:
        print(f"Client handling error: {e}")
    finally:
        client_socket.close()  # Close the client socket when done

# Function to start the server and listen for incoming connections
def start_server(host, port):
    try:
        server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server_socket.bind((host, port))
        server_socket.listen()
        print(f"Server listening on {host}:{port}")

        while True:
            # Accept a new client connection and create a new thread to handle it
            client_socket, client_address = server_socket.accept()
            print(f"Accepted connection from {client_address[0]}:{client_address[1]}")
            client_thread = threading.Thread(target=handle_client, args=(client_socket,))
            client_thread.start()  # Start the new thread
    except Exception as e:
        print(f"Server error: {e}")
    finally:
        server_socket.close()  # Close the server socket when done

def parse_args():
    # Function to parse command-line arguments
    parser = argparse.ArgumentParser(description="Multithreaded server with IP and port options")
    parser.add_argument("-ip", "--host", default='127.0.0.1', help="Server IP address")
    parser.add_argument("-p", "--port", type=int, default=12345, help="Server port number")
    return parser.parse_args()

if __name__ == "__main__":
    args = parse_args()
    HOST = args.host  # Use the specified host or the default (127.0.0.1)
    PORT = args.port  # Use the specified port or the default (12345)

    start_server(HOST, PORT)
