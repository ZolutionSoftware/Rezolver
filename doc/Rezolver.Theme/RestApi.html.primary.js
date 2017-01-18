var restApiCommon = require('./RestApi.common.js');

exports.transform = function (model) {
  model = restApiCommon.transform(model);
  model._disableToc = model._disableToc || !model._tocPath || (model._navPath === model._tocPath);
  return model;
}
