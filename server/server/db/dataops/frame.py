from sqlalchemy.exc import DatabaseError

from server.db.dataops.vehicle import get_vehicle
from server.db.models import Frame, Vehicle, db


def create_frame(vehicle: Vehicle, time: float, path_camera1: str, path_camera2: str,
                 path_camera3: str) -> Frame | None:
    if get_latest_frame_from_vehicle(vehicle) is not None and time < get_latest_frame_from_vehicle(vehicle).timestamp:
        print("Time is less than last frame time. Skipping frame creation.")
        return None
    frame = Frame(vehicle=vehicle, path_camera1=path_camera1, path_camera2=path_camera2, path_camera3=path_camera3)
    db.session.add(frame)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error creating frame. Rolling back. Error:', e)
        db.session.rollback()
        raise
    return frame


def create_frame_from_msg(message: str) -> Frame:
    _, map_id, vehicle_id, _, time, path_camera1, path_camera2, path_camera3 = message.split(' ')
    vehicle = get_vehicle(int(map_id), int(vehicle_id))
    return create_frame(vehicle, float(time), path_camera1, path_camera2, path_camera3)


def get_frame(frame_id: int) -> Frame:
    return Frame.query.filter_by(id=frame_id).one_or_404()


def get_frames_by_ids(map_id: int, vehicle_id: int) -> list[Frame]:
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).all()


def get_frames(vehicle: Vehicle) -> list[Frame]:
    return Frame.query.filter_by(vehicle=vehicle).all()


def get_latest_frame_by_ids(map_id: int, vehicle_id: int) -> Frame | None:
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).order_by(Frame.id.desc()).first()


def get_latest_frame_from_vehicle(vehicle: Vehicle) -> Frame | None:
    return Frame.query.filter_by(vehicle=vehicle).order_by(Frame.id.desc()).first()


def get_nth_frame_by_ids(map_id: int, vehicle_id: int, n: int) -> Frame | None:
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).order_by(Frame.id.desc()).offset(n - 1).first()


def get_nth_frame_by_vehicle(vehicle: Vehicle, n: int) -> Frame | None:
    return Frame.query.filter_by(vehicle=vehicle).order_by(Frame.id.desc()).offset(n - 1).first()


def delete_frame(frame: Frame) -> None:
    db.session.delete(frame)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error deleting frame. Rolling back. Error:', e)
        db.session.rollback()
        raise


def delete_frame_by_id(frame_id: int) -> None:
    frame = get_frame(frame_id)
    delete_frame(frame)
