from sqlalchemy.exc import DatabaseError

from server.db.models import Map, db


def create_map(map_id: int, seed: int, aerial_view_path: str) -> Map:
    map_obj = Map(id=map_id, seed=seed, aerial_view_path=aerial_view_path)
    db.session.add(map_obj)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error creating map. Rolling back. Error:', e)
        db.session.rollback()
        raise
    return map_obj


def get_map(map_id: int) -> Map:
    return Map.query.filter_by(id=map_id).one_or_404()


def get_all_maps() -> list[Map]:
    return Map.query.all()


def delete_map(map_obj: Map) -> None:
    db.session.delete(map_obj)
    try:
        db.session.commit()
    except DatabaseError as e:
        print('Error deleting map. Rolling back. Error:', e)
        db.session.rollback()
        raise


def delete_map_by_id(map_id: int) -> None:
    map_obj = get_map(map_id)
    delete_map(map_obj)
