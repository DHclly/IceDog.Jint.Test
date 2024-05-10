// this import js path relative the main.js file path

import Person from './Person.js';
import * as lodash from '../old/lodash.js';// just import  lodash.js,not lodash-es

let p1 = new Person(`tom`, 20);
logger.info(p1.introSelf());
logger.info(p1.greet(`jerry`));
let arr = _.map([1, 2, 3,4], x => x * x);
logger.info(JSON.stringify(arr));

logger.info(`lodash version:${_.VERSION}`);

export const returnVal = 1;