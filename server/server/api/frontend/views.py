from flask import Blueprint, render_template
from server.db.dataops.map import get_all_maps
from server.db.dataops.vehicle import get_vehicles

frontend_blueprint = Blueprint('frontend_blueprint', __name__)


@frontend_blueprint.route('/simulations')
def simulations():
    template_file_path = "menu.html"

    simulations = get_all_maps()
    vehicles_per_map = [get_vehicles(sim.id) for sim in simulations]

    return render_template(template_file_path, simulations=simulations, vehicles=vehicles_per_map)


@frontend_blueprint.route('/simulations/<int:simulation_id>/<int:vehicle_id>')
def instance(simulation_id: int, vehicle_id: int):
    template_file_path = "simulation.html"

    title = "Simulation " + str(simulation_id)

    return render_template(template_file_path, title=title, mapId=simulation_id, vehicleId=vehicle_id)
