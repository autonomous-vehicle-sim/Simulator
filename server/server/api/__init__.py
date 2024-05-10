from server.api.common import api, blueprint
from server.api.default.views import api as default

api.add_namespace(default, '/')
