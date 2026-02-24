import Alpine from 'alpinejs';
import gamesList from '../components/games-list.js';
import headerSearch from '../components/header-search.js';
import accountSettings from '../components/account-settings.js';

Alpine.data('gamesList', gamesList);
Alpine.data('headerSearch', headerSearch);
Alpine.data('accountSettings', accountSettings);

Alpine.start();