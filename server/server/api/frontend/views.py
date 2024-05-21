from flask import Blueprint, render_template
from server.db.dataops.map import get_all_maps

frontend_blueprint = Blueprint('frontend_blueprint', __name__)


@frontend_blueprint.route('/simulations')
def simulations():
    template_file_path = "menu.html"

    simulations = get_all_maps()

    return render_template(template_file_path, simulations=simulations)


@frontend_blueprint.route('/simulations/<simulation_id>')
def instance(simulation_id: str):
    template_file_path = "simulation.html"

    title = "Simulation " + simulation_id
    camera_views = []

    return render_template(template_file_path, title=title, camera_views=camera_views, mapId=simulation_id)
