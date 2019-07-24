/// <binding BeforeBuild='default' Clean='cleanDocFXOutput' />
/// AfterBuild='sitemap'
var gulp = require('gulp');
var cleancss = require('gulp-clean-css');
var concat = require('gulp-concat');
var less = require('gulp-less');
var sourcemaps = require('gulp-sourcemaps');
var del = require('del');
var merge = require('merge-stream');
var LessAutoprefix = require('less-plugin-autoprefix');

var autoprefix = new LessAutoprefix({ browsers: ['last 2 versions'] });

var sitemap = require('gulp-sitemap');

//copied only
var bootstrap = {
    src: ['**/*'], cwd: 'bower_components/bootstrap/less/',
    font: {
        src: ['*'],
        cwd: 'bower_components/bootstrap/dist/fonts/'
    }
};

var fontawesome = {
    src: ['**/*'], cwd: 'bower_components/components-font-awesome/less/',
    font: {
        src: ['*'],
        cwd: 'bower_components/components-font-awesome/fonts/'
    }
};
//built
var rezolverBootstrap = {
    src: 'site.less',
    cwd: './wwwroot/css/'
};
var rezolverDocFXMain = {
    src: 'main.less',
    dest: ['main.*'],
    cwd: './_docfx_themes/rezolver/styles/'
};

//bundling and minification of the docfx custom template theme - the
//files which replace the default docfx.vendor.* files.
var vendor = {
    css: ['wwwroot/css/site.css' //built from site.less in less task after copy
        //includes: custom bootstrap, 
        //custom highlightjs theme and font-awesome
    ],
    js: ['bower_components/jquery/dist/jquery.min.js',
        'bower_components/bootstrap/dist/js/bootstrap.min.js',
        'bower_components/highlightjs/highlight.pack.min.js',
        'bower_components/lunr.js/lunr.min.js',
        'bower_components/js-url/url.min.js',
        'bower_components/twbs-pagination/jquery.twbsPagination.min.js',
        "bower_components/mark.js/dist/jquery.mark.min.js",
        "bower_components/anchor-js/anchor.min.js"
    ],
    dest: ['docfx.vendor.*']
};

var devSitemap = {
    src: ['./wwwroot/developers/**/*.html', '!./wwwroot/developers/**/toc.html'],
    dest: './wwwroot/developers',
    siteUrl: 'http://rezolver.co.uk/developers'
};

gulp.task('sitemap', function () {
    return gulp.src(devSitemap.src, {
        read: false
    })
        .pipe(sitemap({
            siteUrl: devSitemap.siteUrl
        }))
        .pipe(gulp.dest(devSitemap.dest));
});

//destroys all the output and intermediate files produced by the docfx processing - this is to ensure that
//APIs and pages which are no longer being produced are no longer present in the content.
gulp.task('cleanDocFXOutput', function () {
    return del(['./wwwroot/developers/**', '!./developers']);
});

gulp.task('cleanDocFXLog', function () {
    return del(['./log.txt']);
});

gulp.task('copy', function () {
    return merge(
        gulp.src(bootstrap.src, { cwd: bootstrap.cwd })
            .pipe(gulp.dest('./wwwroot/css/bootstrap/')),
        gulp.src(fontawesome.src, { cwd: fontawesome.cwd })
            .pipe(gulp.dest('./wwwroot/css/fontawesome/')),
        //fonts are copied to two places to satisfy both the main site's
        //styles and the Rezolver docfx theme's styles.
        gulp.src(bootstrap.font.src, { cwd: bootstrap.font.cwd })
            .pipe(gulp.dest('./wwwroot/css/fonts'))
            .pipe(gulp.dest('./_docfx_themes/rezolver/styles/fonts/')),
        gulp.src(fontawesome.font.src, { cwd: fontawesome.font.cwd })
            //fonts are copied to two places to satisfy both the main site's
            //styles and the Rezolver docfx theme's styles.
            .pipe(gulp.dest('./wwwroot/css/fonts'))
            .pipe(gulp.dest('./_docfx_themes/rezolver/styles/fonts/'))
    );
});

//single 'less' task compiles our less files with source maps
gulp.task('less', ['copy'], function () {
    return merge(
        //builds the rezolver bootstrap theme from the less in ./styles/
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

gulp.task('bundles', ['less'], function () {
    return merge(
        gulp.src(vendor.css)
            .pipe(sourcemaps.init())
            .pipe(cleancss())
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

//the default task - single dependencies are used to chain all the tasks together
gulp.task('default', ['cleanDocFXLog', 'bundles']);

//used only when performing style updates to the Rezolver docfx
//theme or underlying bootstrap theme whilst running the site - runs the default
//task and then copies everything to the /developers/styles folder - which is usually
//performed by the docfx build courtesy of its template handling.
gulp.task('patchtemplate', ['default'], function () {
    return gulp.src([].concat(rezolverDocFXMain.dest).concat(vendor.dest), { cwd: rezolverDocFXMain.cwd })
        .pipe(gulp.dest('./wwwroot/developers/styles'));
});