from flask import Blueprint
from flask_restx import Api


class FixedApi(Api):
    def ns_urls(self, ns, urls):
        def fix(url):
            return url[1:] if url.startswith('//') else url

        return [fix(url) for url in super().ns_urls(ns, urls)]


blueprint = Blueprint('api', __name__, url_prefix='/api')
api = FixedApi(blueprint)
