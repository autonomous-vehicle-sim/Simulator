from flask_restx import fields
from server.api.common import api

ControlEngineCommand = api.model('ControlEngineCommand', {
    'engine': fields.Integer(required=True, description='Acceleration of the vehicle'),
})

ControlSteeringCommand = api.model('ControlSteeringCommand', {
    'steering': fields.Float(required=False, description='Steering angle of the vehicle'),
})

ControlPositionCommand = api.model('ControlPositionCommand', {
    'position': fields.List(fields.Integer, required=False, description='Position of the vehicle'),
})

InitMap = api.model('InitMap', {
    'seed': fields.Integer(required=False, description='Seed for the map generation'),
})

InitInstance = api.model('InitInstance', {
    'map_id': fields.Integer(required=True, description='Map ID'),
    'max_steer': fields.Integer(required=True, description='Max steering angle'),
    'max_engine': fields.Integer(required=True, description='Max engine power'),
    'pos_x': fields.Integer(required=False, description='Initial X position of the vehicle'),
    'pos_y': fields.Integer(required=False, description='Initial Y position of the vehicle'),
})
