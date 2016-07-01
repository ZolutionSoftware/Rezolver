/// <binding BeforeBuild='default' />
module.exports = function (grunt) {
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        less: {
            dev: {
                options: {
                    sourceMap: true,
                    sourceMapFilename: 'site.debug.css.map',
                    sourceMapUrl: 'site.debug.css.map',
                    //sourceMapBasepath: 'styles',
                    dumpLineNumbers: 'comments',
                    relativeUrls: true,
                    plugins: [
                        new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
                    ]
                },
                files: {
                    'styles/site.debug.css': 'styles/site.less'
                }
            },
            prod: {
                options: {
                    sourceMap: true,
                    sourceMapFilename: 'site.css.map',
                    sourceMapUrl: 'site.css.map',
                    //sourceMapBasepath: 'styles',
                    sourceMapRootpath: '/styles',
                    compress: true,
                    relativeUrls: true,
                    plugins: [
                            new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
                    ]
                },
                files: {
                    'styles/site.css': 'styles/site.less'
                }
            }
        },
        copy: {
            bootstrap_less: {
                expand: true,
                cwd: 'bower_components/bootstrap/less',
                src: '**',
                dest: 'styles/bootstrap'
            },
            bootstrap_fonts: {
                expand: true,
                cwd: 'bower_components/bootstrap/fonts',
                src: '**',
                dest: 'styles/fonts'
            }
        }
    });

    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-copy');

    grunt.registerTask('default', ['copy', 'less']);
    grunt.registerTask('prod', ['copy', 'less:prod']);
    grunt.registerTask('dev', ['copy', 'less:dev']);
};