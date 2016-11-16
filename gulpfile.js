var gulp = require('gulp');
var marked = require('gulp-marked');
var replace = require('gulp-replace');
var rename = require('gulp-rename');
var fs = require('fs');

var articlePath = './article/';
var tempPath = './temp/';

var mdFileNameToNormalName = {};
var createdDateToFileName = {};
var createdDates = [];

gulp.task('splitmarkdown', function () {
    fs.readdirSync(tempPath).forEach(function (file, index) {
        var curPath = tempPath + file;
        fs.unlinkSync(curPath);
    });
    var body = fs.readFileSync('blog.md', 'utf8');
    var bodies = body.split("---\n");
    var name = 0;
    bodies.map(function (val) {
        var title = val.replace(/^\s+|\s+$/g, '');

        var normalName = title
            .substring(3, title.indexOf('\n'));

        var splittedNormalName = normalName.split('- ');
        var createdDate = new Date(splittedNormalName[splittedNormalName.length - 1]);
        createdDates.push(createdDate);

        var nameOfFile = normalName
            .replace(/[\!\"\#\$\%\&\'()\*\+\,.\/\:\;\<\=\>\?\@\^\_\`\{\|\}\~-]/g, '')
            .replace(/[\s]/g, '_')
            .replace(/_+/g, '_');

        createdDateToFileName[createdDate] = nameOfFile;
        mdFileNameToNormalName[nameOfFile] = normalName;

        fs.writeFileSync(tempPath + nameOfFile + '.md', val);
    });

    createdDates.sort(function (a, b) {
        if (a < b) {
            return 1;
        } else if (a == b) {
            return 0;
        } else {
            return -1;
        }
    })
});

gulp.task('markdownv2', ['splitmarkdown'], function () {
    return gulp.src(tempPath + '*.md')
        .pipe(marked())
        .pipe(gulp.dest(tempPath));
});

gulp.task('replaceArticle', ['markdownv2'], function () {
    fs.readdirSync(articlePath).forEach(function (file, index) {
        var curPath = articlePath + file;
        fs.unlinkSync(curPath);
    });
    var template = fs.readFileSync('articleTemplate.html', 'utf8');
    fs.readdirSync(tempPath).forEach(function (file, index) {
        if (file.endsWith('html')) {
            var curPath = tempPath + file;
            var val = fs.readFileSync(curPath, 'utf8');
            fs.writeFileSync(articlePath + file, template.replace(/\{BODY\}/g, val));
        }
    });
});

gulp.task('indexBody', ['replaceArticle'], function () {
    var indexBodyArray = {};
    fs.readdirSync(articlePath).forEach(function (file, index) {
        if (file.endsWith('html')) {
            var curPath = articlePath + file;
            var linkText = file.replace(/\.html$/g, '');
            if (mdFileNameToNormalName[linkText]) {
                indexBodyArray[linkText] = '<div><a href="' + curPath + '#disqus_thread">' + mdFileNameToNormalName[linkText] + '</a></div>';
            }
        }
    });

    var indexBody = '';
    for (var i=0; i< createdDates.length; i++)
    {
        var nameOfFile = createdDateToFileName[createdDates[i]];
        indexBody +=  indexBodyArray[nameOfFile];
    }

    fs.writeFileSync('indexBody.html', indexBody);
});

gulp.task('replaceIndex', ['indexBody'], function () {
    return gulp.src('./indexTemplate.html')
        .pipe(replace(/{BODY}/, function (s) {
            var body = fs.readFileSync('indexBody.html', 'utf8');
            return body;
        }))
        .pipe(rename('./index.html'))
        .pipe(gulp.dest('./'));
});

gulp.task('default', ['splitmarkdown', 'markdownv2', 'replaceArticle', 'indexBody', 'replaceIndex']);
