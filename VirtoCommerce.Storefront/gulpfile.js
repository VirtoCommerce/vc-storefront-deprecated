/// <binding BeforeBuild='default' Clean='clean' ProjectOpened='watch' />

var gulp = require('gulp'),

    inject = require('gulp-inject'),
    filter = require('gulp-filter'),
    concat = require('gulp-concat'),
    replace = require('gulp-replace'),
    rename = require('gulp-rename'),
    clean = require('gulp-clean'),
    del = require('del'),

    mergestream = require('merge-stream'),
    sequence = require('run-sequence'),
    util = require('gulp-util'), // preserve to be able output custom messages in future

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
        
    zip = require('gulp-zip');

var regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/,
    ext: /\.([^\.]+)$/
};

var rootPath = 'App_Data/Themes/default/';

function getPackage() {
    delete require.cache[require.resolve('./package.json')];
    return require('./package.json');
}

function getBundleConfig() {
    delete require.cache[require.resolve('./bundleconfig.json')];
    return require('./bundleconfig.json');
}

function merge(streams) {
    return streams.length ? mergestream(streams) : mergestream().end();
}

gulp.task('min', ['min:js', 'min:css', 'min:html']);

function mapSources() {
    return sourcemaps.mapSources(function (sourcePath, file) {
        var sourceRootPathEndIndex = sourcePath.indexOf('assets');
        var sourceRootPath = sourcePath.substring(0, sourceRootPathEndIndex);
        var relativeRootPath = '../'.repeat((sourceRootPath.match(new RegExp('/', "g")) || []).length) + sourcePath.substring(sourceRootPathEndIndex);
        return relativeRootPath;
    });
}

gulp.task('min:js', function () {
    var tasks = getBundles(regex.js).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(sourcemaps.init())
            .pipe(mapSources())
            .pipe(concat(bundle.outputFileName))
            .pipe(uglify({ mangle: false }))
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('min:css', function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(sourcemaps.init())
            .pipe(mapSources())
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
                }),
                cssnano()
            ]))
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('min:html', function () {
    var tasks = getBundles(regex.html).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(concat(bundle.outputFileName))
            //.pipe(htmlmin({ collapseWhitespace: true, minifyCSS: true, minifyJS: true }))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('clean', function () {
    var files = [].concat.apply([], getBundleConfig().map(function (bundle) {
        var fileName = bundle.outputFileName;
        return [fileName, fileName.replace(regex.ext, '.$1.map')];
    }));

    return del(files);
});

gulp.task('watch', function () {
    gulp.watch('./bundleconfig.json', ['min', 'snippet']);
    gulp.watch('./package.json', ['snippet']);

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
    return getBundleConfig().filter(function (bundle) {
        return regexPattern.test(bundle.outputFileName);
    });
}

gulp.task('lint', function () {
    var tasks = getBundles(regex.js).filter(function(bundle) { return !bundle.disableLint || bundle.disableLint === undefined }).map(function(bundle) {
        return gulp.src(bundle.inputFiles, { base: '.' })
            .pipe(eslint("./.eslintrc.json"))
            .pipe(eslint.format());
    });
    return merge(tasks);
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
        addSuffix: '?ver=' + name + 'BuildVersion',
        ignorePath: ignorePath
    };
}

gulp.task('snippet:js', function () {
    var package = getPackage();
    var tasks = getBundles(regex.js).map(function(bundle) {
        return gulp.src('bundle.liquid')
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Debug', 'script_tag', null, rootPath + 'assets/static/')))
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Release', 'script_tag', null, rootPath + 'assets/static/')))
            .pipe(replace('?ver=DebugBuildVersion\'', '?ver=\' + ("now" | date: "%s")'))
            .pipe(replace('?ver=ReleaseBuildVersion', '?ver=' + package.version))
            .pipe(rename(bundle.outputFileName))
            .pipe(rename({ dirname: rootPath + 'snippets/bundle', extname: '.liquid' }))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('snippet:css', function () {
    var package = getPackage();
    var tasks = getBundles(regex.css).map(function(bundle) {
        return gulp.src('bundle.liquid')
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Debug', 'stylesheet_tag', null, rootPath + 'assets/static/')))
            .pipe(inject(gulp.src([bundle.outputFileName], { read: false }), defaultOptions('Release', 'stylesheet_tag', null, rootPath + 'assets/static/')))
            .pipe(replace('?ver=DebugBuildVersion\'', '?ver=\' + ("now" | date: "%s")'))
            .pipe(replace('?ver=ReleaseBuildVersion', '?ver=' + package.version))
            .pipe(rename(bundle.outputFileName))
            .pipe(rename({ dirname: rootPath + 'snippets/bundle', extname: '.liquid' }))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
});

gulp.task('release', ['min', 'snippet'], function () {
    util.log("execured release");
    var package = getPackage();
    return gulp.src([].concat([rootPath + '/**'], [].concat.apply([], getBundleConfig().map(function (bundle) { return bundle.inputFiles.map(function (inputFile) { return '!' + inputFile; }) }))))
        .pipe(zip(package.name + '-' + package.version + '.zip'))
        .pipe(gulp.dest('.'));
});

gulp.task('snippet', ['snippet:js', 'snippet:css']);

// DEFAULT Tasks
gulp.task('default', function(callback) {
    sequence('lint', ['min', 'snippet'], callback);
});
