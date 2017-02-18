module.exports = function (grunt) {
  //don't currently need any of this now: gulp is doing it all :)
    //grunt.initConfig({
    //    pkg: grunt.file.readJSON('package.json'),
    //    clean: {
    //        all: ['_docfx_themes/rezolver/styles/main.less','styles/*.map', 'developers/**', 'log.txt']
    //    }
    //  //,
    //  //  copy: {
    //  //      bootstrap_less: {
    //  //          expand: true,
    //  //          cwd: 'bower_components/bootstrap/less',
    //  //          src: '**',
    //  //          dest: 'styles/bootstrap'
    //  //      },
    //  //      bootstrap_fonts: {
    //  //          expand: true,
    //  //          cwd: 'bower_components/bootstrap/fonts',
    //  //          src: '**',
    //  //          dest: 'styles/fonts'
    //  //      }
    //  //  },
    //  //  site_less: {
    //  //      dev: {
    //  //          options: {
    //  //              sourceMap: true,
    //  //              sourceMapFilename: 'styles/site.debug.css.map',
    //  //              sourceMapURL: 'site.debug.css.map',
    //  //              sourceMapBasepath: 'styles/',
    //  //              dumpLineNumbers: 'comments',
    //  //              relativeUrls: true,
    //  //              plugins: [
    //  //                  new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
    //  //              ]
    //  //          },
    //  //          files: {
    //  //            'styles/site.debug.css': 'styles/site.less',
                  
    //  //          }
    //  //      },
    //  //      prod: {
    //  //          options: {
    //  //              sourceMap: true,
    //  //              sourceMapFilename: 'styles/site.css.map',
    //  //              sourceMapURL: 'site.css.map',
    //  //              sourceMapBasepath: 'styles/',
    //  //              compress: true,
    //  //              relativeUrls: true,
    //  //              plugins: [
    //  //                      new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
    //  //              ]
    //  //          },
    //  //          files: {
    //  //            'styles/site.css': 'styles/site.less',
    //  //            'styles/highlight-theme-rezolver.css': 'styles/highlight-theme-rezolver.less'
    //  //          }
    //  //      }
    //  //  },
    //  //  highlight_less: {
    //  //    dev: {
    //  //      options: {
    //  //        sourceMap: true,
    //  //        sourceMapFilename: 'styles/site.debug.css.map',
    //  //        sourceMapURL: 'site.debug.css.map',
    //  //        sourceMapBasepath: 'styles/',
    //  //        dumpLineNumbers: 'comments',
    //  //        relativeUrls: true,
    //  //        plugins: [
    //  //            new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
    //  //        ]
    //  //      },
    //  //      files: {
    //  //        'styles/site.debug.css': 'styles/site.less',
    //  //        'styles/highlight-theme-rezolver.debug.css': 'styles/highlight-theme-rezolver.less'
    //  //      }
    //  //    },
    //  //    prod: {
    //  //      options: {
    //  //        sourceMap: true,
    //  //        sourceMapFilename: 'styles/site.css.map',
    //  //        sourceMapURL: 'site.css.map',
    //  //        sourceMapBasepath: 'styles/',
    //  //        compress: true,
    //  //        relativeUrls: true,
    //  //        plugins: [
    //  //                new (require('less-plugin-autoprefix'))({ browsers: ["last 2 versions"] })
    //  //        ]
    //  //      },
    //  //      files: {
    //  //        'styles/highlight-theme-rezolver.debug.css': 'styles/highlight-theme-rezolver.less',
    //  //        'styles/highlight-theme-rezolver.css': 'styles/highlight-theme-rezolver.less'
    //  //      }
    //  //    }
    //  //  }
    //});

    //grunt.loadNpmTasks('grunt-contrib-clean');
    //grunt.loadNpmTasks('grunt-contrib-less');
    //grunt.loadNpmTasks('grunt-contrib-copy');

    //grunt.registerTask('default', ['copy', 'site-less']);
    //grunt.registerTask('prod', ['copy', 'site-less:prod']);
    //grunt.registerTask('dev', ['copy', 'site_less:dev']);
};