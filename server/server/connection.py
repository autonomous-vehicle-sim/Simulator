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
        self.__loop = asyncio.new_event_loop()
        self.__stop = self.__loop.create_future()
        self.__message_lock = threading.Lock()
        self.__websocket_server = None
        self.__message_queue = queue.Queue()

    def get_message(self) -> str:
        self.__message_lock.acquire()
        message = self.__message_queue.get()
        self.__message_lock.release()
        print(f"Message: {message}")
        return message

    def send_and_get_message(self, message) -> str:
        self.__message_lock.acquire()
        self.send_message(message)
        response = self.__message_queue.get()
        self.__message_lock.release()
        print(f"Message: {response}")
        return message

    async def __send_message(self, message) -> None:
        if self.__websocket_server is None:
            print("ERROR: Simulator is not connected")
            raise ConnectionError("Simulator is not connected")
        else:
            try:
                print(f"Sending message: {message}")
                await self.__websocket_server.send(message)
            except websockets.exceptions.ConnectionClosedError:
                print("ERROR: Simulator has disconnected")
                self.__websocket_server = None
                self.stop()

    def send_message(self, message) -> None:
        task = self.__loop.create_task(self.__send_message(message))
        self.__loop.run_until_complete(task)

    async def __handle_connection(self, websocket, path) -> None:
        print("Simulator has connected")
        self.__websocket_server = websocket
        try:
            async for message in websocket:
                self.__message_lock.acquire()
                self.__message_queue.put(message)
                self.__message_lock.release()
        except websockets.exceptions.ConnectionClosedError:
            print("Simulator has disconnected")
            self.__websocket_server = None
        finally:
            self.stop()

    async def __start_server(self) -> None:
        print("Starting server...")
        async with websockets.serve(self.__handle_connection, IP_ADDRESS, PORT):
            await self.__stop
        print("Server stopped")

    def start(self) -> None:
        asyncio.run_coroutine_threadsafe(self.__start_server(), self.__loop)

    def stop(self) -> None:
        self.__stop.set_result(None)
