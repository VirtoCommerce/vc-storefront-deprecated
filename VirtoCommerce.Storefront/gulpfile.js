/// <binding />
'use strict';

var gulp = require('gulp'),

    inject = require('gulp-inject'),
    filter = require('gulp-filter'),
    rename = require('gulp-rename'),
    concat = require('gulp-concat'),
    clean = require('gulp-clean'),
    replace = require('gulp-replace'),
    merge = require('merge-stream'),
    del = require('del'),
    util = require('gulp-util'), // preserve to be able output custom messages in future

    bundleconfig = require('./bundleconfig.json'),
    uglify = require('gulp-uglify'),
    bourbon = require('node-bourbon'),
    autoprefixer = require('autoprefixer'),
    cssnano = require('cssnano'),
    postcss = require('gulp-postcss'),
    sass = require('gulp-sass'),
    htmlmin = require('gulp-htmlmin'),
    imagemin = require('gulp-image'),
    sourcemaps = require('gulp-sourcemaps'),

    eslint = require('gulp-eslint'),

    uuid = require('uuid/v4');

var regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/,
    ext: /\.([^\.]+)$/
};

var buildUid = uuid().split('-')[4];

gulp.task('bundle', ['bundle:js', 'bundle:css', 'min:html']);

gulp.task('bundle:js', ['pack:js', 'min:js']);
gulp.task('bundle:css', ['pack:css', 'min:css']);

gulp.task('pack', ['pack:js', 'pack:css']);

function packJs(bundle) {
    return gulp.src(bundle.inputFiles, { base: '.' })
        .pipe(sourcemaps.init())
        .pipe(concat(bundle.outputFileName));
}

gulp.task('pack:js', function () {
    var tasks = getBundles(regex.js).map(function(bundle) {
        return packJs(bundle)
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

function packCss(bundle) {
    return gulp.src(bundle.inputFiles, { base: '.' })
        .pipe(sourcemaps.init())
        .pipe(concat(bundle.outputFileName))
        .pipe(postcss([
            autoprefixer({
                browsers: [
                    'Explorer >= 10',
                    'Edge >= 12',
                    'Firefox >= 19',
                    'Chrome >= 20',
                    'Safari >= 8',
                    'Opera >= 15',
                    'iOS >= 8',
                    'Android >= 4.4',
                    'ExplorerMobile >= 10',
                    'last 2 versions'
                ]
            })
        ]));
}

gulp.task('pack:css', function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
        return packCss(bundle)
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('min', ['min:js', 'min:css', 'min:html']);

gulp.task('min:js', function () {
    var tasks = getBundles(regex.js).map(function (bundle) {
        return packJs(bundle)
            .pipe(uglify({ mangle: false }))
            .pipe(rename({ extname: '.min.js' }))
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('min:css', function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
        return packCss(bundle)
            .pipe(postcss([cssnano()]))
            .pipe(rename({ extname: '.min.css' }))
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('min:html', function () {
    var tasks = getBundles(regex.html).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(concat(bundle.outputFileName))
            .pipe(htmlmin({ collapseWhitespace: true, minifyCSS: true, minifyJS: true }))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('clean', function () {
    var files = [].concat.apply([], bundleconfig.map(function (bundle) {
        var fileName = bundle.outputFileName;
        return [fileName, fileName.replace(regex.ext, '.min.$1'), fileName.replace(regex.ext, '.min.$1.map')];
    }));

    return del(files);
});

gulp.task('watch', function () {
    getBundles(regex.js).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ['bundle:js']);
    });

    getBundles(regex.css).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ['bundle:css']);
    });

    getBundles(regex.html).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ['min:html']);
    });
});

function getBundles(regexPattern) {
    return bundleconfig.filter(function (bundle) {
        return regexPattern.test(bundle.outputFileName);
    });
}

gulp.task('lint', function () {
    getBundles(regex.js).forEach(function (bundle) {
        if (!bundle.disableLint || bundle.disableLint === undefined)
        {
            gulp.src(bundle.inputFiles, { base: '.' })
                .pipe(eslint())
                .pipe(eslint.format());
        }
    });
});

function defaultOptions(name, tagName, prefix, ignorePath) {
    tagName = tagName || 'script_tag';
    prefix = prefix || '';
    return {
        addRootSlash: false,
        removeTags: true,
        starttag: '<!-- inject:' + name + ' -->',
        transform: function (filepath) {
            return '{{ \'' + filepath + '\' | static_asset_url | ' + tagName + ' }}';
        },
        addPrefix: prefix,
        addSuffix: '?ver=BUILDVERSION',
        ignorePath: ignorePath
    };
}

gulp.task('snippet:js', ['bundle:js'], function () {
    getBundles(regex.js).forEach(function (bundle) {
        return gulp.src('bundle.liquid')
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Debug', 'script_tag', '', 'App_Data/Themes/default/assets/static/')))
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }).pipe(rename({ extname: '.min.js' })), defaultOptions('Release', 'script_tag', '', 'App_Data/Themes/default/assets/static/')))
            .pipe(replace('?ver=BUILDVERSION', '?ver=' + buildUid))
            .pipe(rename(bundle.outputFileName))
            .pipe(rename({ dirname: 'App_Data/Themes/default/snippets/bundle', extname: '.liquid' }))
            .pipe(gulp.dest('.'));
    });
});

gulp.task('snippet:css', ['bundle:css'], function () {
    getBundles(regex.css).forEach(function (bundle) {
        return gulp.src('bundle.liquid')
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Debug', 'stylesheet_tag', '', 'App_Data/Themes/default/assets/static/')))
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }).pipe(rename({ extname: '.min.css' })), defaultOptions('Release', 'stylesheet_tag', '', 'App_Data/Themes/default/assets/static/')))
            .pipe(replace('?ver=BUILDVERSION', '?ver=' + buildUid))
            .pipe(rename(bundle.outputFileName))
            .pipe(rename({ dirname: 'App_Data/Themes/default/snippets/bundle', extname: '.liquid' }))
            .pipe(gulp.dest('.'));
    });
});

gulp.task('snippet', ['snippet:js', 'snippet:css']);

// DEFAULT Tasks
gulp.task('default', ['lint', 'bundle', 'snippet']);
