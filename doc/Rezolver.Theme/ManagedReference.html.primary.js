var mrefCommon = require('./ManagedReference.common.js');

exports.transform = function (model)  {
  model = mrefCommon.transform(model);
  model._disableToc = model._disableToc || !model._tocPath || (model._navPath === model._tocPath);
  return {item: model};
}