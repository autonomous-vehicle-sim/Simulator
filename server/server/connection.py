import asyncio
import queue
import threading

import websockets
import websockets.exceptions

from server.consts import WS_ADDRESS, WS_PORT

IP_ADDRESS = WS_ADDRESS
PORT = WS_PORT


class WSConnection:
    def __init__(self):
        self.__stop = asyncio.Future()
        self.__message_lock = threading.Lock()
        self.__websocket_server = None
        self.__message_queue = queue.Queue()

    def get_message(self) -> str:
        self.__message_lock.acquire()
        message = None
        if not self.__message_queue.empty():
            message = self.__message_queue.get()
        self.__message_lock.release()
        print(f"Message: {message}")
        return message

    async def _send_message(self, message) -> None:
        pass

    def send_message(self, message) -> None:
        asyncio.create_task(self._send_message(message))

    async def __handle_connection(self, websocket, path) -> None:
        pass

    async def __start_server(self) -> None:
        print("Starting server...")
        async with websockets.serve(self.__handle_connection, IP_ADDRESS, PORT):
            await self.__stop
        print("Server stopped")

    def start(self) -> None:
        asyncio.create_task(self.__start_server())

    def stop(self) -> None:
        self.__stop.set_result(None)
