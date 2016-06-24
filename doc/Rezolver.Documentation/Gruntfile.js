/// <binding BeforeBuild='default' />
module.exports = function (grunt) {
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        less: {
            dev: {
                options: {
                    sourceMap: true,
                    dumpLineNumbers: 'comments',
                    relativeUrls: true,
                    plugins: [
                        new (require('less-plugin-autoprefix'))({browsers: ["last 2 versions"]})
                    ]
                },
                files: {
                    'styles/site.debug.css': 'styles/site.less'
                }
            },
            prod: {
                options: {
                    sourceMap: true,
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
        }
    });

    grunt.loadNpmTasks('grunt-contrib-less');

    grunt.registerTask('default', ['less']);
    grunt.registerTask('prod', ['less:prod']);
    grunt.registerTask('dev', ['less:dev']);
};