/// <binding />
'use strict';

var gulp = require('gulp'),
    concat = require('gulp-concat'),
    cssmin = require('gulp-cssmin'),
    htmlmin = require('gulp-htmlmin'),
    uglify = require('gulp-uglifyjs'),
    merge = require('merge-stream'),
    del = require('del'),
	bundleconfig = require('./bundleconfig.json'),
    eslint = require('gulp-eslint'),
    inject = require('gulp-inject'),
    filter = require('gulp-filter'),
    util = require('gulp-util'),
    using = require('gulp-using'),
    replace = require('gulp-replace'),
    clean = require('gulp-clean'),
    uuid = require('uuid/v4'),
    sass = require('gulp-sass'),
    bourbon = require('node-bourbon'),
    autoprefixer = require('gulp-autoprefixer'),
    imagemin = require('gulp-image'),
	wait = require('gulp-wait'),
	sourcemaps = require('gulp-sourcemaps'),
	rename = require('gulp-rename');

var regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/
};

const buildUid = uuid().split('-')[4];

gulp.task('min', ['min:js', 'min:css', 'min:html']);

gulp.task('min:js', function () {
	var tasks = getBundles(regex.js).map(function (bundle) {
		return gulp.src(bundle.inputFiles, { base: '.' })
			.pipe(concat(bundle.outputFileName))
			.pipe(gulp.dest('.'))
			.pipe(rename({ extname: '.min.js' }))
			.pipe(uglify({
				mangle: false,
				outSourceMap: true
			}))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('min:css', function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
		return gulp.src(bundle.inputFiles, { base: '.' })
			.pipe(concat(bundle.outputFileName))
			.pipe(gulp.dest('.'))
			.pipe(rename({ extname: '.min.css' }))
			.pipe(cssmin({ sourceMap: true}))
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
    var files = bundleconfig.map(function (bundle) {
        return bundle.outputFileName;
    });

    return del(files);
});

gulp.task('watch', function () {
    getBundles(regex.js).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ['min:js']);
    });

    getBundles(regex.css).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ['min:css']);
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

gulp.task('snippet:js', function () {
	getBundles(regex.js).forEach(function (bundle) {
		return gulp.src('bundle.liquid')
			.pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Debug', 'script_tag', '', 'App_Data/Themes/default/assets/static/')))
			.pipe(inject(gulp.src([bundle.outputFileName], { read: false }).pipe(rename({ extname: '.min.js' })), defaultOptions('Release', '', '', 'App_Data/Themes/default/assets/static/')))
			.pipe(replace('?ver=BUILDVERSION', '?ver=' + buildUid))
			.pipe(rename(bundle.outputFileName))
			.pipe(rename({ dirname: 'App_Data/Themes/default/snippets/bundle', extname: '.liquid' }))
			.pipe(gulp.dest('.'));
	});
});

gulp.task('snippet:css', function () {
	getBundles(regex.css).forEach(function (bundle) {
		return gulp.src('bundle.liquid')
			.pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Debug', 'stylesheet_tag', '', 'App_Data/Themes/default/assets/static/')))
			.pipe(inject(gulp.src([bundle.outputFileName], { read: false }).pipe(rename({ extname: '.min.css' })), defaultOptions('Release', '', '', 'App_Data/Themes/default/assets/static/')))
			.pipe(replace('?ver=BUILDVERSION', '?ver=' + buildUid))
			.pipe(rename(bundle.outputFileName))
			.pipe(rename({ dirname: 'App_Data/Themes/default/snippets/bundle', extname: '.liquid' }))
			.pipe(gulp.dest('.'));
	});
});

gulp.task('snippet', ['snippet:js', 'snippet:css']);

// DEFAULT Tasks
gulp.task('default', ['lint', 'min', 'snippet']);
