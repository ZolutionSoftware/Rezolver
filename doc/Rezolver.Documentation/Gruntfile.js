/// <binding BeforeBuild='default' />
module.exports = function (grunt) {
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        clean: {
            all: ['styles/*.map', 'learn/**', 'log.txt']
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
        },
        less: {
            dev: {
                options: {
                    sourceMap: true,
                    sourceMapFilename: 'styles/site.debug.css.map',
                    sourceMapURL: 'site.debug.css.map',
                    sourceMapBasepath: 'styles/',
                    dumpLineNumbers: 'comments',
                    relativeUrls: true,
                    plugins: [
                        new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
                    ]
                },
                files: {
                  'styles/site.debug.css': 'styles/site.less',
                  'styles/highlight-theme-rezolver.debug.css': 'styles/highlight-theme-rezolver.less'
                }
            },
            prod: {
                options: {
                    sourceMap: true,
                    sourceMapFilename: 'styles/site.css.map',
                    sourceMapURL: 'site.css.map',
                    sourceMapBasepath: 'styles/',
                    compress: true,
                    relativeUrls: true,
                    plugins: [
                            new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
                    ]
                },
                files: {
                  'styles/site.css': 'styles/site.less',
                  'styles/highlight-theme-rezolver.css': 'styles/highlight-theme-rezolver.less'
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-copy');

    grunt.registerTask('default', ['copy', 'less']);
    grunt.registerTask('prod', ['copy', 'less:prod']);
    grunt.registerTask('dev', ['copy', 'less:dev']);
};