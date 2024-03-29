import socket
import threading
import struct

running = True

messageCounter = 1


def listen_for_exit_command():
    global running
    global messageCounter
    while running:
        cmd = input()
        if cmd == '/message':
            send_message_to_client("Zde je se zprava",
                                   addr)  
        elif cmd == '/bye':
            send_bye_to_client(addr)
        elif cmd == '/err':
            send_err_to_client("Doslo k chybe", addr)
        elif cmd == '/replyok':
            send_replyok_to_client("Authentication successful", addr)
        elif cmd == '/replynok':
            send_replyNok_to_client("NEuspesna REPLY odpoved", addr)
        elif cmd == 'quit':
            print("Ukončuji server...")
            running = False
        messageCounter += 1

def send_message_to_client(message, addr):
    msg = bytearray([0x04])  # 0x04 pro MSG
    msg += struct.pack('!H', messageCounter)
    msg += "ServerAdmin".encode()
    msg += bytearray([0x00])
    msg += message.encode()
    msg += bytearray([0x00])
    sock.sendto(msg, addr)


def send_bye_to_client(addr):
    bye_msg = bytearray([0xFF])  # 0xFF pro BYE
    bye_msg += struct.pack('!H', messageCounter)
    sock.sendto(bye_msg, addr)


def send_err_to_client(error_message, addr):
    err_msg = bytearray([0xFE])
    err_msg += struct.pack('!H', messageCounter)
    err_msg += "ServerAdmin".encode()
    err_msg += bytearray([0x00])
    err_msg += error_message.encode()
    err_msg += bytearray([0x00])
    sock.sendto(err_msg, addr)


def send_replyok_to_client(reply_message, addr):
    reply_msg = (bytearray([0x01]))
    reply_msg += struct.pack('!H', messageCounter)
    reply_msg += (bytearray([0x01]))
    reply_msg += (bytearray([0x00, 0x03]))
    reply_msg += reply_message.encode()
    sock.sendto(reply_msg, addr)


def send_replyNok_to_client(reply_message, addr):
    reply_msg = (bytearray([0x01]))
    reply_msg += struct.pack('!H', messageCounter)
    reply_msg += (bytearray([0x00]))
    reply_msg += (bytearray([0x00, 0x03]))
    reply_msg += reply_message.encode()
    sock.sendto(reply_msg, addr)


host = '0.0.0.0'
port = 4567

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((host, port))
sock.setblocking(False)

print(f"Server naslouchá na {host}:{port}. Napište 'quit' pro ukončení.")

exit_thread = threading.Thread(target=listen_for_exit_command)
exit_thread.start()

try:
    global addr
    while running:
        try:
            data, addr = sock.recvfrom(1024)
            print(f"Přijata zpráva: {data.decode()} od {addr}")

            confirm_message = bytearray([0x00, 0x00, 0x01])  # 0x00 pro CONFIRM
            if data[0] != 0:
                sock.sendto(confirm_message, addr)
                print("Odeslána CONFIRM zpráva klientovi")

        except BlockingIOError:
            continue

except KeyboardInterrupt:
    print("Server ukončen uživatelem")

finally:
    running = False
    sock.close()
    exit_thread.join()
