import { bicep } from './bicep.js'

export default {
    iconLinks: [
        {
            icon: 'github',
            href: 'https://github.com/vipwan/Biwen.QuickApi',
            title: 'GitHub'
        },
        {
            icon: 'twitter',
            href: 'https://www.cnblogs.com/vipwan/',
            title: 'Myblogs'
        }
    ],
    start() {
        console.log('started');
    },
    configureHljs(hljs) {
        hljs.registerLanguage('bicep', bicep);
    },
}

