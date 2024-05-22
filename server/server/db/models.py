from flask_sqlalchemy import SQLAlchemy

db = SQLAlchemy()


class Map(db.Model):
    __tablename__ = 'map'
    id = db.Column(db.Integer, primary_key=True, autoincrement=False)
    seed = db.Column(db.Integer, nullable=False)


class Vehicle(db.Model):
    __tablename__ = 'vehicle'
    map_id = db.Column(db.Integer, db.ForeignKey('map.id', ondelete='CASCADE'), primary_key=True, autoincrement=False)
    vehicle_id = db.Column(db.Integer, primary_key=True, autoincrement=False)
    map = db.relationship('Map', backref=db.backref('vehicles', cascade='all, delete-orphan'),
                          foreign_keys=[map_id])
    engine = db.Column(db.Float, nullable=False)
    steer = db.Column(db.Float, nullable=False)
    last_update = db.Column(db.Numeric, nullable=False)


class Frame(db.Model):
    __tablename__ = 'frame'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    map_id = db.Column(db.Integer, db.ForeignKey('vehicle.map_id', ondelete='CASCADE'))
    vehicle_id = db.Column(db.Integer, db.ForeignKey('vehicle.vehicle_id', ondelete='CASCADE'))
    path_camera1 = db.Column(db.String(255), nullable=False)
    path_camera2 = db.Column(db.String(255), nullable=False)
    path_camera3 = db.Column(db.String(255), nullable=False)
    timestamp = db.Column(db.Numeric, nullable=False)
    vehicle = db.relationship('Vehicle', backref=db.backref('frames', cascade='all, delete-orphan'),
                              primaryjoin="and_(Frame.map_id == Vehicle.map_id,"
                                          "Frame.vehicle_id == Vehicle.vehicle_id)",
                              foreign_keys=[map_id, vehicle_id])
