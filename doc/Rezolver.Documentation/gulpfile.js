/// <binding BeforeBuild='default' />
// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.
var path = require('path');
var gulp = require('gulp');
var minify = require('gulp-minify-css');
var rename = require('gulp-rename');
var concat = require('gulp-concat');
var copy = require('gulp-copy');
var less = require('gulp-less');
var sourcemaps = require('gulp-sourcemaps');
var es = require('event-stream');
var LessAutoprefix = require('less-plugin-autoprefix');

var autoprefix = new LessAutoprefix({ browsers: ['last 2 versions'] });

//copied only
var bootstrap = {
  src: ['**/*'], cwd: 'bower_components/bootstrap/less/',
  font: {
    src: ['*'],
    cwd: 'bower_components/bootstrap/dist/fonts/'
  }
};
//built
var rezolverBootstrap = {
    src: 'site.less',
    cwd: 'styles/'
};
var rezolverDocFXMain = {
  src: 'main.less',
  cwd: '_docfx_themes/rezolver/styles/'
};

//bundling and minification of the docfx custom template theme - the
//files which replace the default docfx.vendor.* files.
var vendor = {
  css: [/*'bower_components/highlightjs/styles/github-gist.css',*/
        'styles/site.css' //built from site.less in less task after copy
                          //includes custom bootstrap and custom highlightjs theme
  ],
  js: ['bower_components/jquery/dist/jquery.min.js',
       'bower_components/bootstrap/dist/js/bootstrap.min.js',
       'bower_components/highlightjs/highlight.pack.min.js',
       'bower_components/lunr.js/lunr.min.js',
       'bower_components/js-url/url.min.js',
       'bower_components/twbs-pagination/jquery.twbsPagination.min.js',
       "bower_components/mark.js/dist/jquery.mark.min.js",
       "bower_components/anchor-js/anchor.min.js"
  ]
}

gulp.task('less', function () {
  return es.merge(
    gulp.src(rezolverBootstrap.src, { cwd: rezolverBootstrap.cwd })
      .pipe(sourcemaps.init())
      .pipe(less({
        plugins: [autoprefix]
      }))
      .pipe(sourcemaps.write('.'))
      .pipe(gulp.dest(rezolverBootstrap.cwd)),
    gulp.src(rezolverDocFXMain.src, { cwd: rezolverDocFXMain.cwd })
      .pipe(sourcemaps.init())
      .pipe(less({
        plugins: [autoprefix]
      }))
      .pipe(sourcemaps.write('.'))
      .pipe(gulp.dest(rezolverDocFXMain.cwd))
    );
});

gulp.task('copy', function () {
  return es.merge(
    gulp.src(bootstrap.src, { cwd: bootstrap.cwd })
      .pipe(copy('./styles/bootstrap/')),
    gulp.src(bootstrap.font.src, { cwd: bootstrap.font.cwd })
      //fonts are copied to two places to satisfy both the main site's
      //styles and the Rezolver docfx theme's styles.
      .pipe(copy('./styles/fonts'))
      .pipe(copy('./_docfx_themes/rezolver/styles/fonts/'))
  );
});


gulp.task('bundles', function () {
  return es.merge(
    gulp.src(vendor.css)
      .pipe(sourcemaps.init())
      .pipe(minify({ keepBreaks: true }))
      .pipe(rename({
        suffix: '.min'
      }))
      .pipe(concat('docfx.vendor.css'))
      .pipe(sourcemaps.write('.'))
      .pipe(gulp.dest(rezolverDocFXMain.cwd)),
    gulp.src(vendor.js)
      .pipe(sourcemaps.init())
      .pipe(concat('docfx.vendor.js'))
      .pipe(sourcemaps.write('.'))
      .pipe(gulp.dest(rezolverDocFXMain.cwd))
  );
});



gulp.task('default', ['copy', 'less', 'bundles', ]);
