import os

from flask import Flask

from server.api import blueprint, api
from server.consts import UPLOAD_FOLDER


def create_app(test_config=None):
    app = Flask(__name__, instance_relative_config=True)
    app.config.from_mapping(
        SECRET_KEY='dev',
        DATABASE=os.path.join(app.instance_path, 'small_object_counter.sqlite'),
    )

    if test_config is None:
        app.config.from_pyfile('config.py', silent=True)
    else:
        app.config.from_mapping(test_config)

    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    app.config['UPLOAD_FOLDER'] = app.instance_path + UPLOAD_FOLDER

    api.init_app(app, add_specs=False)
    app.register_blueprint(blueprint)

    return app
