from flask import Blueprint, render_template

frontend_blueprint = Blueprint('frontend_blueprint', __name__)


@frontend_blueprint.route('/simulations')
def simulations():
    template_file_path = "menu.html"
    return render_template(template_file_path)


@frontend_blueprint.route('/simulations/<simulation_id>')
def instance(simulation_id: str):
    template_file_path = "simulation.html"

    return render_template(template_file_path)
