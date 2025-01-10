// Init zoom object
function zoomWrapper(parent, childClass = "") {
    // Parent group
    let parentEl = $(parent);
    // Get child
    let childZoomAble = parentEl.find("img");
    if (childClass.trim() !== "") {
        childZoomAble = parentEl.find("img." + childClass);
    }
    let process = Object.keys(childZoomAble)
        .filter(function(key) {
            return !isNaN(Number(key));
        })
        .map(function(key) {
            return childZoomAble[key];
        });
    return processImage(process);
}

function loadImage(url) {
    return new Promise((resolve) => {
        let img = new Image();
        img.onload = () => resolve(true);
        img.onerror = () =>  resolve(false);
        img.src = url;
    });
}

async function delayedLog(item) {
    let srcImg = $(item).attr("src");
    await loadImage(srcImg).then(rs => {
        if (rs === false) {
            return;
        }
        let bound = document.createElement('a')
        bound.href = srcImg;
        bound.className = "zoom wrapper";
        bound.style = "display: inline-table;";
        bound.innerHTML = item.outerHTML;
        item.replaceWith(bound);
    }).catch(error => {
        console.log(error);
    });
}
async function processImage(array) {
    for (const item of array) {
        await delayedLog(item);
    }
}

