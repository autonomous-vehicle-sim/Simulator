import asyncio
import queue
import threading
import traceback

import requests
import websockets
import websockets.exceptions

from server.consts import WS_ADDRESS, WS_PORT
from server.config import PORT as FLASK_PORT
from server.db.dataops.frame import create_frame_from_msg

IP_ADDRESS = WS_ADDRESS
PORT = WS_PORT


class WSConnection:
    _instance = None

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
            cls._instance.__initialized = False
        return cls._instance

    def __init__(self):
        if self.__initialized:
            return
        self.__stop = None
        self.__message_lock = threading.Lock()
        self.__websocket_server = None
        self.__message_queue = queue.Queue()
        self.__initialized = True

    def is_running(self) -> bool:
        return self.__stop is not None and not self.__stop.done()

    def get_message(self) -> str:
        self.__message_lock.acquire()
        message = self.__message_queue.get()
        self.__message_lock.release()
        print(f"Message: {message}")
        return message

    def send_and_get_message(self, message) -> str:
        self.__message_lock.acquire()
        asyncio.run(self.send_message(message))
        response = self.__message_queue.get()
        self.__message_lock.release()
        print(f"Message: {response}")
        return response

    async def send_message(self, message) -> None:
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

    async def __handle_connection(self, websocket, path) -> None:
        print("Simulator has connected")
        await websocket.send("Connected")
        self.__websocket_server = websocket
        try:
            async for message in websocket:
                if message.startswith("screen"):
                    try:
                        frame = create_frame_from_msg(message)
                        print(f"Frame created: {frame}")
                    except Exception as e:
                        print(f"Error creating frame, skipping: {e}")
                        traceback.print_exc()
                    finally:
                        continue
                elif message.startswith("engine"):
                    pass
                elif message.startswith("steer"):
                    pass
                self.__message_queue.put(message)
        except websockets.exceptions.ConnectionClosedError:
            print("Simulator has disconnected.")
            self.__websocket_server = None
        finally:
            self.stop()

    async def __start_server(self) -> None:
        print("Starting WS server...")
        async with websockets.serve(self.__handle_connection, IP_ADDRESS, PORT) as ws:
            print(f"WS server started at {ws.sockets[0].getsockname()[0]}:{ws.sockets[0].getsockname()[1]}")
            await self.__stop
        print("WS server stopped")
        requests.post(f"http://localhost:{FLASK_PORT}/shutdown")

    def start(self) -> None:
        if self.is_running():
            return
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        self.__stop = loop.create_future()
        loop.create_task(self.__start_server())
        loop.run_forever()

    def stop(self) -> None:
        print("Stopping WS server...")
        self.__stop.set_result(True)
