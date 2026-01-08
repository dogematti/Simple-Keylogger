import socket

SERVER_IP = "0.0.0.0"
SERVER_PORT = 5555

def handle_client(conn, addr):
    print(f"Connection from {addr}")

    # Turn the socket into a file-like object so we can readline()
    f = conn.makefile("r", encoding="utf-8", newline="\n")

    try:
        for line in f:
            msg = line.rstrip("\r\n")
            if not msg:
                continue
            print(f"Received: {msg}")
            conn.sendall(b"ok\n")
    except (ConnectionResetError, BrokenPipeError):
        pass
    finally:
        try:
            f.close()
        except:
            pass
        conn.close()
        print(f"Disconnected {addr}")

def main():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.bind((SERVER_IP, SERVER_PORT))
        s.listen()
        print(f"Listening on {SERVER_IP}:{SERVER_PORT}...")

        while True:
            conn, addr = s.accept()
            # Simple single-thread handling (blocks while connected)
            handle_client(conn, addr)

if __name__ == "__main__":
    main()
