from flask_sqlalchemy import SQLAlchemy

db = SQLAlchemy()


class Map(db.Model):
    __tablename__ = 'map'
    id = db.Column(db.Integer, primary_key=True)
    seed = db.Column(db.Integer, nullable=False)


class Vehicle(db.Model):
    __tablename__ = 'vehicle'
    map_id = db.Column(db.Integer, db.ForeignKey('map.id', ondelete='CASCADE'), primary_key=True)
    vehicle_id = db.Column(db.Integer, primary_key=True)
    map = db.relationship('Map', backref=db.backref('vehicles', cascade='all, delete-orphan'))


class Frame(db.Model):
    __tablename__ = 'frame'
    id = db.Column(db.Integer, primary_key=True)
    map_id = db.Column(db.Integer, db.ForeignKey('vehicle.map_id', ondelete='CASCADE'))
    vehicle_id = db.Column(db.Integer, db.ForeignKey('vehicle.vehicle_id', ondelete='CASCADE'))
    path_camera1 = db.Column(db.String(255), nullable=False)
    path_camera2 = db.Column(db.String(255), nullable=False)
    path_camera3 = db.Column(db.String(255), nullable=False)
    vehicle = db.relationship('Vehicle', backref=db.backref('frames', cascade='all, delete-orphan'))
