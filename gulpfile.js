var gulp = require('gulp');
var marked = require('gulp-marked');
var replace = require('gulp-replace');
var rename = require('gulp-rename');
var fs = require('fs');

var articlePath = './article/';
var tempPath = './temp/';

var colours = ["#FFEC94", "#FFAEAE", "#FFF0AA", "#B0E57C", "#B4D8E7", "#56BAEC"];

var allTags = [];
var coloursIndex = 0;

var mdFileNameToNormalName = {};
var mdFileNameToTags = {};
var createdDateToFileName = {};
var createdDates = [];

gulp.task('splitmarkdown', function() {
    function readingTags(body, name) {
        var firstLineEnd = body.indexOf('\n');
        var secondLine = body.substring(firstLineEnd + 1, body.indexOf('\n', firstLineEnd + 1)).replace(/^\s+|\s+$/g, '');
        if (secondLine.indexOf('Tags: ') === 0) {
            secondLine.substring(6).split(',').map(x => x.replace(/^\s+|\s+$/g, '')).map(tag => {
                var tagObj = allTags.find(x => x.Tag === tag);
                if (!tagObj) {
                    tagObj = { Tag: tag, Colour: colours[(coloursIndex++) % colours.length] };
                    allTags.push(tagObj);
                }
                if (!mdFileNameToTags[name]) mdFileNameToTags[name] = [];
                mdFileNameToTags[name].push(tagObj);
            });
        }

    }
    fs.readdirSync(tempPath).forEach(function(file, index) {
        var curPath = tempPath + file;
        fs.unlinkSync(curPath);
    });
    var body = fs.readFileSync('blog.md', 'utf8');
    var bodies = body.split(/---\r?\n/);
    var name = 0;
    bodies.map(function(val) {
        var cleanBody = val.replace(/^\s+|\s+$/g, '');

        var normalName = cleanBody
            .substring(3, cleanBody.indexOf('\n'));

        var splittedNormalName = normalName.split('- ');
        var createdDate = new Date(splittedNormalName[splittedNormalName.length - 1]);
        createdDate.setHours(23);
        while (createdDates.some(x => x.toLocaleString() == createdDate.toLocaleString())) {
            createdDate.setHours(createdDate.getHours() - 1);
        }

        createdDates.push(createdDate);

        var nameOfFile = normalName
            .replace(/[\!\"\#\$\%\&\'()\*\+\,.\/\:\;\<\=\>\?\@\^\_\`\{\|\}\~-]/g, '')
            .replace(/[\s]/g, '-')
            .replace(/-+/g, '-');

        createdDateToFileName[createdDate] = nameOfFile;
        mdFileNameToNormalName[nameOfFile] = normalName;
        readingTags(cleanBody, nameOfFile);

        fs.writeFileSync(tempPath + nameOfFile + '.md', val);
    });

    createdDates.sort(function(a, b) {
        if (a < b) {
            return 1;
        } else if (a == b) {
            return 0;
        } else {
            return -1;
        }
    })
});

gulp.task('markdownv2', ['splitmarkdown'], function() {
    return gulp.src(tempPath + '*.md')
        .pipe(marked({ langPrefix: '' }))
        .pipe(gulp.dest(tempPath));
});

gulp.task('replaceArticle', ['markdownv2'], function() {
    fs.readdirSync(articlePath).forEach(function(file, index) {
        var curPath = articlePath + file;
        fs.unlinkSync(curPath);
    });
    var template = fs.readFileSync('articleTemplate.html', 'utf8');
    fs.readdirSync(tempPath).forEach(function(file, index) {
        if (file.endsWith('html')) {
            var curPath = tempPath + file;
            var val = fs.readFileSync(curPath, 'utf8');
            var tags = mdFileNameToTags[file.replace('.html', '')];
            fs.writeFileSync(articlePath + file, template.replace(/\{BODY\}/g, val)
                .replace(/\{PAGE_TITLE\}/g, mdFileNameToNormalName[file.replace('.html', '')])
                .replace(/\{TAGS\}/g, tags ? tags.map(x => x.Tag).join() : "")
            );
        }
    });
});

gulp.task('indexBody', ['replaceArticle'], function() {
    var indexBodyArray = {};
    fs.readdirSync(articlePath).forEach(function(file, index) {
        if (file.endsWith('html')) {
            var curPath = articlePath + file;
            var linkText = file.replace(/\.html$/g, '');
            if (mdFileNameToNormalName[linkText]) {
                var tagDivs = '<div class="tag">';
                if (mdFileNameToTags[linkText]) {
                    for (var i = 0; i < mdFileNameToTags[linkText].length; i++) {
                        tagDivs += '<div style="background-color:' + mdFileNameToTags[linkText][i].Colour + '">' + mdFileNameToTags[linkText][i].Tag + '</div>';
                    }
                }
                tagDivs += '</div>';

                var normalName = mdFileNameToNormalName[linkText].split(" - ")[0];
                var date = mdFileNameToNormalName[linkText].split(" - ")[1];
                indexBodyArray[linkText] = '<div><div class="date">' + date + '</div><a href="' + curPath + '">' + normalName + '</a>' + tagDivs + '</div>';
            }
        }
    });

    var indexBody = '';
    for (var i = 0; i < createdDates.length; i++) {
        var nameOfFile = createdDateToFileName[createdDates[i]];
        indexBody += indexBodyArray[nameOfFile];
    }

    fs.writeFileSync('indexBody.html', indexBody);
});

gulp.task('replaceIndex', ['indexBody'], function() {
    return gulp.src('./indexTemplate.html')
        .pipe(replace(/{BODY}/, function(s) {
            var body = fs.readFileSync('indexBody.html', 'utf8');
            return body;
        }))
        .pipe(rename('./index.html'))
        .pipe(gulp.dest('./'));
});

gulp.task('default', ['splitmarkdown', 'markdownv2', 'replaceArticle', 'indexBody', 'replaceIndex']);