import os

from flask import Flask, redirect

from server.api.common import blueprint, api
from server.api.frontend.views import frontend_blueprint
from server.consts import DB_NAME
from server.utils import config_db


def create_app(test_config=None):
    app = Flask(__name__, instance_relative_config=True, template_folder='templates/')
    app.config.from_mapping(
        SECRET_KEY='dev',
        DATABASE=os.path.join(app.instance_path, 'server.sqlite'),
    )

    if test_config is None:
        app.config.from_pyfile('config.py', silent=True)
    else:
        app.config.from_mapping(test_config)

    try:
        os.makedirs(app.instance_path)
    except OSError:
        pass

    config_db(app, DB_NAME)

    @app.route('/shutdown', methods=['POST'])
    def shutdown():
        os._exit(0)

    @app.route('/')
    def index():
        return redirect('/simulations', code=301)

    api.init_app(app, add_specs=False)
    app.register_blueprint(blueprint)
    app.register_blueprint(frontend_blueprint)

    return app


app = create_app()
