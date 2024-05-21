from flask import Blueprint, render_template

frontend_blueprint = Blueprint('frontend_blueprint', __name__)


@frontend_blueprint.route('/simulations')
def simulations():
    template_file_path = "menu.html"



    return render_template(template_file_path)


@frontend_blueprint.route('/simulations/<simulation_id>')
def instance(simulation_id: str):
    template_file_path = "simulation.html"

    title = "Simulation " + simulation_id
    camera_views = []

    return render_template(template_file_path, title=title, camera_views=camera_views, mapId=simulation_id)
