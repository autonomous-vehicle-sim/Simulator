from enum import Enum


class MessageGetType(Enum):
    STEER = 1
    ENGINE = 2
    CAMERA = 3


class MessageSetType(Enum):
    STEER = 1
    ENGINE = 2
    POSITION = 3


def create_set_message(map_id: int, instance_id: int, message_type: MessageSetType, value: int) -> str:
    if message_type == MessageSetType.POSITION:
        raise NotImplementedError("Set position is not implemented on the server")
    if value < -100 or value > 100:
        raise ValueError("Value must be between -100 and 100")
    return f"{map_id} {instance_id} {message_type.value} {value}"


def create_get_message(map_id: int, instance_id: int, message_type: MessageGetType,
                       camera_id: int = None, image_id: int = None) -> str:
    if message_type == MessageGetType.CAMERA:
        if camera_id is None or image_id is None:
            raise ValueError("Camera message requires camera_id and image_id")
        return f"{map_id} {instance_id} {message_type.value} {camera_id} {image_id}"
    return f"{map_id} {instance_id} {message_type.value}"


def create_delete_message(map_id: int, instance_id: int = None) -> str:
    if instance_id is None:
        return f"{map_id} delete"
    return f"{map_id} {instance_id} delete"


def create_init_instance_message(map_id: int, max_steer, max_engine: int, pos_x: int = 0, pos_y: int = 0) -> str:
    return f"{map_id} init_new {max_steer} {max_engine} {pos_x} {pos_y}"


def create_init_map_message(seed: int = -1):
    return f"init_new_map {seed}"
