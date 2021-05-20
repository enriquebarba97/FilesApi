import axios from "axios";
import authHeader from "./auth-header";

const API_URL = process.env.REACT_APP_API_URL;


class FileService {
    getOwnFiles(username){
        return axios.get(API_URL + username + "/files", { headers: {'Authorization': authHeader()}});
    }

    getSharedFiles(username){
        return axios.get(API_URL + username + "/files/shared", { headers: {'Authorization': authHeader()}});
    }

    getFile(filename, username){
        return axios.get(API_URL + username + "/files/"+ filename, { headers: {'Authorization': authHeader()}, responseType: 'blob'});
    }

    uploadFile(file, username){
        return axios.post(API_URL + username + "/files", file, {headers: {'Authorization': authHeader(), 'Content-Type': "multipart/form-data"}})
    }

    deleteFile(filename, username){
        return axios.delete(API_URL + username + "/files/"+ filename, { headers: {'Authorization': authHeader()}});
    }

    addShare(filename, username, userShare){
        return axios.get(API_URL + username + "/files/"+ filename+ "/share", {params:{share:userShare}, headers: {'Authorization': authHeader()}});
    }
}

export default new FileService();