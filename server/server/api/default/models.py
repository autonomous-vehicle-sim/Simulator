from flask_restx import fields
from server.api.common import api

ControlEngineCommand = api.model('ControlEngineCommand', {
    'map_id': fields.Integer(required=True, description='Map ID'),
    'vehicle_id': fields.Integer(required=True, description='Vehicle ID'),
    'engine': fields.Integer(required=True, description='Acceleration of the vehicle'),
})

ControlSteeringCommand = api.model('ControlSteeringCommand', {
    'map_id': fields.Integer(required=True, description='Map ID'),
    'vehicle_id': fields.Integer(required=True, description='Vehicle ID'),
    'steering': fields.Float(required=False, description='Steering angle of the vehicle'),
})

ControlPositionCommand = api.model('ControlPositionCommand', {
    'map_id': fields.Integer(required=True, description='Map ID'),
    'vehicle_id': fields.Integer(required=True, description='Vehicle ID'),
    'position': fields.List(fields.Integer, required=False, description='Position of the vehicle'),
})
