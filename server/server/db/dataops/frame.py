from sqlalchemy.exc import DatabaseError

from server.db.models import Vehicle, db


def insert_updated_vehicle_state(message: str) -> Vehicle:
    _, map_id, vehicle_id, frame_index, time, path_camera1, path_camera2, path_camera3, steer, engine = message.split(';')
    updated_state = Vehicle(map_id=int(map_id), vehicle_id=int(vehicle_id), frame_index=frame_index,
                            path_camera1=path_camera1, path_camera2=path_camera2, path_camera3=path_camera3,
                            steer=float(steer.replace(",", ".")), engine=float(engine.replace(",", ".")),
                            last_update=float(time.replace(",", ".")))
    try:
        db.session.add(updated_state)
        db.session.commit()
        return updated_state
    except DatabaseError as e:
        print('Error updating vehicle. Rolling back. Error:', e)
        db.session.rollback()
        raise


def get_latest_frame_by_ids(map_id: int, vehicle_id: int) -> Vehicle | None:
    return Vehicle.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).order_by(Vehicle.frame_index.desc()).first()


def get_nth_frame_by_ids(map_id: int, vehicle_id: int, n: int) -> Vehicle | None:
    return (Vehicle.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).order_by(Vehicle.frame_index.desc())
            .offset(n - 1).first())
