import { goodNum, greet } from './tool.js'
export default class Person {
    constructor(name, age) {
        this.name = name;
        this.age = age;
        this.goodNum = goodNum;
    }

    introSelf() {
        return `name:${this.name},age:${this.age},likeNum:${this.goodNum}`
    }

    greet(name) {
        return greet(name);
    }
}