from sqlalchemy.exc import DatabaseError

from server.db.dataops.map import get_map
from server.db.models import Vehicle, Map, db


def create_vehicle(map_obj: Map) -> Vehicle:
    vehicle = Vehicle(map=map_obj)
    db.session.add(vehicle)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error creating vehicle. Rolling back. Error:', e)
        db.session.rollback()
        raise
    return vehicle


def create_vehicle(map_id: int) -> Vehicle:
    map_obj = get_map(map_id)
    return create_vehicle(map_obj)


def get_vehicle(map_id: int, vehicle_id: int) -> Vehicle:
    return Vehicle.query.filter_by(map_id=map_id, vehicle_id=vehicle_id).one_or_404()


def delete_vehicle(vehicle: Vehicle):
    db.session.delete(vehicle)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error deleting vehicle. Rolling back. Error:', e)
        db.session.rollback()
        raise


def delete_vehicle(map_id: int, vehicle_id: int):
    vehicle = get_vehicle(map_id, vehicle_id)
    delete_vehicle(vehicle)
