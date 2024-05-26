from sqlalchemy.exc import DatabaseError

from server.db.dataops.map import get_map
from server.db.models import Vehicle, Map, db


def create_vehicle(map_obj: Map) -> Vehicle:
    latest_id = Vehicle.query.filter_by(map_id=map_obj.id).order_by(Vehicle.vehicle_id.desc()).first()
    if latest_id is not None:
        vehicle_id = latest_id.vehicle_id + 1
    else:
        vehicle_id = 0
    vehicle = Vehicle(map=map_obj, vehicle_id=vehicle_id, engine=0, steer=0, last_update=0)
    db.session.add(vehicle)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error creating vehicle. Rolling back. Error:', e)
        db.session.rollback()
        raise
    return vehicle


def create_vehicle_by_map_id(map_id: int) -> Vehicle:
    map_obj = get_map(map_id)
    return create_vehicle(map_obj)


def get_vehicle(map_id: int, vehicle_id: int) -> Vehicle | None:
    return Vehicle.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).one_or_404()


def get_vehicles(map_id: int) -> list[Vehicle]:
    return Vehicle.query.filter_by(map_id=map_id, frame_index=None).all()


def update_vehicle(vehicle: Vehicle, engine: float | None, steer: float | None, time: float | None) -> None:
    if engine is not None:
        vehicle.engine = engine
    if steer is not None:
        vehicle.steer = steer
    if time is not None:
        vehicle.last_update = time
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error updating vehicle. Rolling back. Error:', e)
        db.session.rollback()
        raise


def update_vehicle_from_msg(message: str) -> None:
    msg_type, map_id, vehicle_id, value, time = message.split(' ')
    vehicle = get_vehicle(int(map_id), int(vehicle_id))
    if msg_type == 'engine':
        engine = value.replace(',', '.')
        steer = vehicle.steer
    elif msg_type == 'steer':
        engine = vehicle.engine
        steer = value.replace(',', '.')
    else:
        print('Invalid message type. Skipping update.')
        return
    update_vehicle(vehicle, float(engine), float(steer), float(time.replace(',', '.')))


def delete_vehicle(vehicle: Vehicle) -> None:
    db.session.delete(vehicle)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error deleting vehicle. Rolling back. Error:', e)
        db.session.rollback()
        raise


def delete_vehicle_by_id(map_id: int, vehicle_id: int) -> None:
    vehicle = get_vehicle(map_id, vehicle_id)
    delete_vehicle(vehicle)
