from sqlalchemy.exc import DatabaseError

from server.db.dataops.vehicle import get_vehicle
from server.db.models import Frame, Vehicle, db


def create_frame(vehicle: Vehicle, path_camera1: str, path_camera2: str, path_camera3: str) -> Frame:
    frame = Frame(vehicle=vehicle, path_camera1=path_camera1, path_camera2=path_camera2, path_camera3=path_camera3)
    db.session.add(frame)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error creating frame. Rolling back. Error:', e)
        db.session.rollback()
        raise
    return frame


def create_frame(message: str) -> Frame:
    _, map_id, vehicle_id, _, _, path_camera1, path_camera2, path_camera3 = message.split(' ')
    vehicle = get_vehicle(int(map_id), int(vehicle_id))
    return create_frame(vehicle, path_camera1, path_camera2, path_camera3)


def get_frame(map_id: int, vehicle_id: int, frame_id: int) -> Frame:
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id, id=frame_id).one_or_404()


def get_frame(frame_id: int) -> Frame:
    return Frame.query.filter_by(id=frame_id).one_or_404()


def get_frame(vehicle: Vehicle, frame_id: int) -> Frame:
    return Frame.query.filter_by(vehicle=vehicle, id=frame_id).one_or_404()


def get_frames(map_id: int, vehicle_id: int):
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).all()


def get_frames(vehicle: Vehicle):
    return Frame.query.filter_by(vehicle=vehicle).all()


def get_latest_frame(map_id: int, vehicle_id: int):
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).order_by(Frame.id.desc()).first()


def get_latest_frame(vehicle: Vehicle):
    return Frame.query.filter_by(vehicle=vehicle).order_by(Frame.id.desc()).first()


def get_nth_frame(map_id: int, vehicle_id: int, n: int):
    return Frame.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).order_by(Frame.id.desc()).offset(n - 1).first()


def get_nth_frame(vehicle: Vehicle, n: int):
    return Frame.query.filter_by(vehicle=vehicle).order_by(Frame.id.desc()).offset(n - 1).first()


def delete_frame(frame: Frame):
    db.session.delete(frame)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error deleting frame. Rolling back. Error:', e)
        db.session.rollback()
        raise


def delete_frame(map_id: int, vehicle_id: int, frame_id: int):
    frame = get_frame(map_id, vehicle_id, frame_id)
    delete_frame(frame)
