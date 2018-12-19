using System;
using System.Collections.Generic;
using System.Device.Location;


namespace algorithm_CS
{
    public class waypoint
    {
        private static List<List<int>> waypoint_global;
        
        public waypoint(){
            waypoint_global = new List<List<int>> ();
        }

        public void add(List<int> tmp){
            waypoint_global.Add(tmp);
        }

        public void delete(int i){
            waypoint_global[i].Add(1);
        }

        public void refresh(){
            foreach(List<int> item in waypoint_global){
                if(item.Count == 3){
                    item.Remove(item[2]);
                }
            }
        }

        public int count(){
            return waypoint_global.Count;
        }
        public List<int> get(int i)
        {
            return waypoint_global[i];
        }

        public List<List<int>> get_all(){
            return waypoint_global;
        }

    }


    public class Program
    {
        // testdata
        // height

        public static DTED_Loader_SQLite_Library.DTED_Loader_Library height_loader = new DTED_Loader_SQLite_Library.DTED_Loader_Library("E:\\Downloads\\DDDDDL\\DTED.db");
        public static int method = 1;
        public static int level_limit = 5;
        public static float penalty = 2;
        public static float velocity = 50;
        public static int size = 100;
        public static waypoint waypoint_global = new waypoint();

        public static void Main(string[] args)
        {
            Console.WriteLine("Algorithm start");
            var input = functions.reading_file("E:\\Google Drive\\Paper\\Algorith implementation\\algorithm_CS\\input_file.txt");
            var map = functions.create_map_risk(input.start, input.target, input.threat);

            List<float> start = new List<float>();
            start.Add(input.start[0]);
            start.Add(input.start[1]);
            start.Add(height_loader.Get_Height(input.start[1], input.start[0]));

            List<float> target = new List<float>();
            target.Add(input.target[0]);
            target.Add(input.target[1]);
            target.Add(height_loader.Get_Height(input.target[1], input.target[0]));

            List<List<int>> waypoint_local = new List<List<int>> ();
           
            foreach(List<float> loc in input.waypoint_cmp){
                List<int> trans_res = functions.transform(loc[0], loc[1], map.Item3.map_init_location, map.Item3.margin_lat, map.Item3.margin_lng);
                waypoint_local.Add(trans_res);
                waypoint_global.add(trans_res);
                loc.AddRange(new List<float> {trans_res[0], trans_res[1]});
            }

            foreach(List<int> aaaaa in waypoint_global.get_all())
            {
                Console.WriteLine((aaaaa[0], aaaaa[1]));
            }


            var all_result = new List<Tuple<float, List<List<float>>>> ();
            for (int z = 1; z <= 20; z += 1)
            {   
                waypoint_global.refresh();

                List<List<int>> raw_result = functions.routing(start, target, z, 0, method, size, waypoint_local, 1, map.location, map.risk, map.Item3.map_init_location, map.Item3.margin_lat, map.Item3.margin_lng, level_limit, penalty);
                List<List<int>> result = new List<List<int>>();
                foreach(List<int> item in raw_result)
                {
                    if(item.Count == 3)
                    {
                        result.Add(item);
                    }
                }

                List<int> start_om = functions.transform(input.start[0], input.start[1], map.Item3.map_init_location, map.Item3.margin_lat, map.Item3.margin_lng);
                start_om.Add((int)(Program.height_loader.Get_Height(start[1], start[0]) / 500));
                List<int> target_om = functions.transform(input.target[0], input.target[1], map.Item3.map_init_location, map.Item3.margin_lat, map.Item3.margin_lng);
                target_om.Add((int)(Program.height_loader.Get_Height(target[1], target[0]) / 500));

                result.Insert(0, start_om);
                result.Add(target_om);

                Tuple<float, List<List<int>>> tmp = new Tuple<float, List<List<int>>>(functions.route_risk(result, map.location, map.risk), result);
                Console.WriteLine(tmp.Item1);
                List<List<float>> loc_ori = new List<List<float>> ();
                foreach (List<int> aaa in tmp.Item2)
                {
                    Console.WriteLine((aaa[0], aaa[1], aaa[2]));
                    foreach (List<float> loc in input.waypoint_cmp)
                    {
                        if (aaa[0] == loc[2] && aaa[1] == loc[3])
                        {
                            loc_ori.Add(new List<float> {loc[0], loc[1], aaa[2] * 500});
                            break;
                        }
                    }
                }
                foreach (List<float> aaaaa in loc_ori)
                {
                    Console.WriteLine((aaaaa[0], aaaaa[1], aaaaa[2]));
                }
                Tuple<float, List<List<float>>> tmp2 = new Tuple<float, List<List<float>>>(tmp.Item1, loc_ori);
                all_result.Add(tmp2);
                Console.ReadLine();
            }

            Console.ReadLine();
            // // // float[,] passpoint = new float[2,4];
            // List<List<int>> tmp = new List<List<int>> ();
            // List<int> tmp2 = new List<int> ();
            
            // Console.WriteLine(tmp[0][1]);
            

        }
    }
    public class functions{

        public static double latlong2DistX(double lastLat, double lastLng, double thisLat, double thisLng)
        {
            // 先緯度再經度
            var sCoord = new GeoCoordinate(lastLat, lastLng);
            var eCoord = new GeoCoordinate(thisLat, thisLng);
            // 回傳兩者距離
            return sCoord.GetDistanceTo(eCoord);
        }

        public static float route_risk(List<List<int>> route, List<List<List<(float lat, float lgt, float hgt)>>> location, List<List<List<float>>> risk_map)
        {
            float total_risk = 0;

            for (int i = 1; i < route.Count; i ++)
            {
                total_risk += path_risk(route[i - 1], route[i], location, risk_map);
            }
            return total_risk;
        }

        public static (float[] start, float[] target, float[,] threat, float[,] feasible, List<List<float>> waypoint_cmp) reading_file(string file_path){
            string[] input = System.IO.File.ReadAllLines(@file_path);
            float[] start = Array.ConvertAll(input[1].Split(','), float.Parse);
            float[] target = Array.ConvertAll(input[2].Split(','), float.Parse);
            float[,] threat = new float[int.Parse(input[0].Split(',')[0]), 6];
            float[,] feasible = new float[int.Parse(input[0].Split(',')[1]), 2];
            List<List<float>> waypoint_cmp = new List<List<float>>();

            bool threat_start_check = false;
            int threat_count = 0;
            bool feasible_start_check = false;
            int feasible_count = 0;

            foreach (string line in input){
                if(line.Split(',')[0] == "threat_start"){
                    threat_start_check = true;
                }else if(line == "threat_end"){
                    threat_start_check = false;
                }else if(threat_start_check){

                    threat[threat_count, 0] = float.Parse(line.Split(',')[0]);
                    threat[threat_count, 1] = float.Parse(line.Split(',')[1]);
                    threat[threat_count, 2] = Program.height_loader.Get_Height(threat[threat_count, 1], threat[threat_count, 0]);
                    threat[threat_count, 3] = float.Parse(line.Split(',')[2]);
                    threat[threat_count, 4] = float.Parse(line.Split(',')[3]);
                    threat[threat_count, 5] = float.Parse(line.Split(',')[4]);
                    threat_count++;
                }

                if(line.Split(',')[0] == "feasible_start"){
                    feasible_start_check = true;
                }else if(line == "feasible_end"){
                    feasible_start_check = false;
                }else if(feasible_start_check){
                    feasible[feasible_count, 0] = float.Parse(line.Split(',')[0]);;
                    feasible[feasible_count, 1] = float.Parse(line.Split(',')[1]);;
                    feasible_count++;

                    List<float> tmp = new List<float>();
                    tmp.Add(float.Parse(line.Split(',')[0]));
                    tmp.Add(float.Parse(line.Split(',')[1]));
                    waypoint_cmp.Add(tmp);
                }
            }

            return (start, target, threat, feasible, waypoint_cmp);

        }

        public static float path_risk(List<int> head, List<int> tail, List<List<List<(float lat, float lgt, float hgt)>>> location, List<List<List<float>>> risk_map){
            float tmp_risk = 0;
            float[] start = {location[head[0]][head[1]][head[2]].lat, location[head[0]][head[1]][head[2]].lgt, location[head[0]][head[1]][head[2]].hgt};
            float[] target = {location[tail[0]][tail[1]][tail[2]].lat, location[tail[0]][tail[1]][tail[2]].lgt, location[tail[0]][tail[1]][tail[2]].hgt};

            float dis = distance(start, target);
            float n = (int)Math.Round(dis / Program.velocity);

            if (dis >= Program.velocity){
                float margin_x = (tail[0] - head[0]) / n;
                float margin_y = (tail[1] - head[1]) / n;
                float margin_z = (tail[2] - head[2]) / n;
               

                for (int i = 0; i <= n; i++){    // 有蒜頭
                    int next_x = (int)Math.Round(head[0] + margin_x * i);
                    int next_y = (int)Math.Round(head[1] + margin_y * i);
                    int next_z = (int)Math.Round(head[2] + margin_z * i);

                    tmp_risk += risk_map[next_x][next_y][next_z];
                }
            }
            return tmp_risk;
        }

        public static float risk(float lat, float lng, float height, float[,] threat){
            // Console.WriteLine(lat);
            // Console.WriteLine(lng);
            // Console.WriteLine(height);
            float risk_sum = 0;
            for(int i = 0; i < threat.GetLength(0); i++){
                float threat_lat = threat[i, 0];
                float threat_lng = threat[i, 1];
                float threat_hgt = threat[i, 2];
                float lethality = threat[i,3];
                float radius = threat[i,4];

                float[] source = {lat, lng, height};
                float[] target = {threat_lat, threat_lng, threat_hgt};
                float dis = distance(source, target);    // distanceeeeeeeeeeeeeeeeeeeeeeeee

                float tmp_risk = (1 - (dis / radius)) * lethality;
                if (tmp_risk > 0){risk_sum += tmp_risk;}
            }
            return risk_sum;
        }

        public static (List<List<List<(float lat, float lgt, float hgt)>>> location, List<List<List<float>>> risk, (float[] map_init_location, float margin_lat, float margin_lng)) create_map_risk(float[] start, float[] target, float[,] threat, int size=100){
            // must dieeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee

            float[] center = {(start[0] + target[0]) / 2, (start[1] + target[1]) / 2};
            float margin_lat = (center[0] - start[0]) / (size / 4);
            float margin_lng = (center[1] - start[1]) / (size / 4);

            if(margin_lat < 0){margin_lat *= -1;}
            if(margin_lng < 0){margin_lng *= -1;}
            if(margin_lat == 0){margin_lat = margin_lng;}
            if(margin_lng == 0){margin_lng = margin_lat;}

            // Console.WriteLine(center[0]);
            // Console.WriteLine(center[1]);
            // Console.WriteLine(margin_lat);
            // Console.WriteLine(margin_lng);
            List<List<List<(float lat, float lgt, float hgt)>>> location = new List<List<List<(float lat, float lgt, float hgt)>>> ();
            List<List<List<float>>> risk_map = new List<List<List<float>>> ();
            float[] map_init_location = {center[0] - margin_lat * 50, center[1] - margin_lng * 50};
            var info = (map_init_location:map_init_location, margin_lat:margin_lat, margin_lng:margin_lng);

            for (int i = 0; i < size; i++){
                List<List<(float lat, float lgt, float hgt)>> location1 = new List<List<(float lat, float lgt, float hgt)>> ();
                List<List<float>> risk1 = new List<List<float>> ();
                for (int j = 0; j < size; j++){
                    List<(float lat, float lgt, float hgt)> location2 = new List<(float lat, float lgt, float hgt)> ();
                    List<float> risk2 = new List<float> ();
                    for (int k = 1; k <= 20; k++){
                        var location3 = (lat: map_init_location[0] + i * margin_lat, lng:map_init_location[1] + j * margin_lng, hgt: (float)(k * 500));
                        // location3.lat = (map_init_location[0] + i * margin_lat);
                        // location3.lng = (map_init_location[1] + j * margin_lng);
                        // location3.hgt = ((float)(2 + 0.5 * k));
                        risk2.Add(risk(location3.lat, location3.lng, location3.hgt, threat));
                        location2.Add(location3);
                    }
                    location1.Add(location2);
                    risk1.Add(risk2);
                }
                location.Add(location1);
                risk_map.Add(risk1);
            }

           return (location, risk_map, info);
        }
        public static float distance(float[] source, float[] target){
            float bot = (float)latlong2DistX(source[0], source[1], target[0], target[1]);
            float height = Math.Abs(source[2] - target[2]);

            return (float)Math.Sqrt(Math.Pow(height, 2) + Math.Pow(bot, 2));
        }
        
        public static List<int> transform(float lat, float lgt, float[] map_init_location, float margin_lat, float margin_lng){
            List<int> x_y = new List<int>();
            int x = (int) Math.Round((lat - map_init_location[0]) / margin_lat);
            int y = (int) Math.Round((lgt - map_init_location[1]) / margin_lng);
            x_y.Add(x);
            x_y.Add(y);

            return x_y;
        }

        public static List<List<int>> routing(List<float> start, List<float> target, int z, int level, int method, int size, List<List<int>> waypoint_local, int priority, List<List<List<(float lat, float lgt, float hgt)>>> location, List<List<List<float>>> risk_map, float[] map_init_location, float margin_lat, float margin_lng, int level_limit, float penalty){
            level += 1;
            
            List<int> start_om = transform(start[0], start[1], map_init_location, margin_lat, margin_lng);
            start_om.Add((int)(Program.height_loader.Get_Height(start[1], start[0]) / 500));
            List<int> target_om = transform(target[0], target[1], map_init_location, margin_lat, margin_lng);
            target_om.Add((int)(Program.height_loader.Get_Height(target[1], target[0]) / 500));

            float min_risk = 100000;
            var min_tmp_risk_list = (new List<float>() {}, 0.0, new List<int>() {1, 2, 3}); 
            
            if (true){
                int start_x = (int) Math.Round((start[0] - map_init_location[0]) / margin_lat);
                int start_y = (int) Math.Round((start[1] - map_init_location[1]) / margin_lng);
                int target_x = (int) Math.Round((target[0] - map_init_location[0]) / margin_lat);
                int target_y = (int) Math.Round((target[1] - map_init_location[1]) / margin_lng);

                if (start_x < target_x){
                    start_x -= (int)size / 5;
                    if (start_x < 0){
                        start_x = 0;
                    }
                    target_x += (int)size / 5;
                    if (target_x > size){
                        target_x = size;
                    }
                }else{
                    target_x -= (int)size / 5;
                    if (target_x < 0){
                        target_x = 0;
                    }
                    start_x += (int)size / 5;
                    if (start_x > size){
                        start_x = size;
                    }
                }
                
                if (start_y < target_y){
                    start_y -= (int)size / 5;
                    if (start_y < 0){
                        start_y = 0;
                    }
                    target_y += (int)size / 5;
                    if (target_y > size){
                        target_y = size;
                    }
                }else{
                    target_y -= (int)size / 5;
                    if (target_y < 0){
                        target_y = 0;
                    }
                    start_y += (int)size / 5;
                    if (start_y > size){
                        start_y = size;
                    }
                }
                float head_x = new float();
                float head_y = new float();
                float tail_x = new float();
                float tail_y = new float();

                if (start_x - target_x > 0){
                    head_x = target_x;
                    tail_x = start_x;
                }
                else{
                    head_x = start_x;
                    tail_x = target_x;
                }
                if (start_y - target_y > 0){
                    head_y = target_y;
                    tail_y = start_y;
                }
                else{
                    head_y = start_y;
                    tail_y = target_y;
                }

                Console.WriteLine("--------------------------------------------------------");
                int min_index = new int();
                var tmp_risk_list = (new List<float>() {}, 0.0, new List<int>() {});
                //foreach(List<int> loc in Program.waypoint_global){
                Console.WriteLine((start_om[0], start_om[1]));
                Console.WriteLine((target_om[0], target_om[1]));

                for (int i = 0; i < waypoint_local.Count; i++) {
                    List<int> loc = Program.waypoint_global.get(i);
                    // Console.WriteLine((loc[0],loc[1]));
                    if (head_x <= loc[0] && loc[0] <= tail_x && head_y <= loc[1] && loc[1] <= tail_y && loc.Count == 2 && Program.height_loader.Get_Height(map_init_location[1] + loc[1] * margin_lng, map_init_location[0] + loc[0] * margin_lat) / 500 < z){

                        loc.Add(z);

                        float first_risk = path_risk(start_om, loc, location, risk_map);
                        float second_risk = path_risk(loc, target_om, location, risk_map);
                        loc.Remove(z);

                        float tmp_risk = 0;
                        if (first_risk > 0)
                        {
                            tmp_risk += first_risk;
                        }
                        if (second_risk > 0)
                        {
                            tmp_risk += second_risk;
                        }
                        
                        Console.WriteLine((loc[0], loc[1], tmp_risk));
                        Boolean check = false;
                        if (tmp_risk <= min_risk)
                        {
                            if (true)
                            {
                                check = true;
                                // if (level == 1){           // check priority situation
                                //     if (waypoint_local.Contains(new List<int>() {loc[0], loc[1], z})){
                                //         check = true;
                                //     }
                                // }else{
                                //     check = true;
                                // }

                                if (check)
                                {
                                    float[] first = { location[start_om[0]][start_om[1]][start_om[2]].lat, location[start_om[0]][start_om[1]][start_om[2]].lgt, location[start_om[0]][start_om[1]][start_om[2]].hgt };
                                    float[] loc_rl = { location[loc[0]][loc[1]][z].lat, location[loc[0]][loc[1]][z].lgt, location[loc[0]][loc[1]][z].hgt };
                                    float[] second = { location[target_om[0]][target_om[1]][target_om[2]].lat, location[target_om[0]][target_om[1]][target_om[2]].lgt, location[target_om[0]][target_om[1]][target_om[2]].hgt };

                                    float total_dis = distance(first, loc_rl) + distance(loc_rl, second);
                                    tmp_risk_list = (new List<float> { loc_rl[0], loc_rl[1], loc_rl[2] }, total_dis, new List<int>() { loc[0], loc[1], z });
                                    min_risk = tmp_risk;
                                    min_index = i;
                                    min_tmp_risk_list = tmp_risk_list;
                                }
                            }
                        }
                    }
           
                }

                Console.WriteLine((min_tmp_risk_list.Item3[0], min_tmp_risk_list.Item3[1], min_tmp_risk_list.Item3[2]));
                Console.ReadLine();

                // if (level == 1){
                //     waypoint_local.Remove(new List<int> {min_tmp_risk_list.Item3[0], min_tmp_risk_list.Item3[1], min_tmp_risk_list.Item3[2]});
                // }

                //Console.WriteLine((start_om[0], start_om[1], start_om[2]));
                //Console.WriteLine((min_tmp_risk_list.Item3[0], min_tmp_risk_list.Item3[1], min_tmp_risk_list.Item3[2]));
                //Console.WriteLine((target_om[0], target_om[1], target_om[2]));
                // float[] first = {location[start_om[0]][start_om[1]][start_om[2]].lat, location[start_om[0]][start_om[1]][start_om[2]].lgt, location[start_om[0]][start_om[1]][start_om[2]].hgt};
                // float[] loc_rl = {location[min_tmp_risk_list.Item3[0]][min_tmp_risk_list.Item3[1]][min_tmp_risk_list.Item3[2]].lat, location[min_tmp_risk_list.Item3[0]][min_tmp_risk_list.Item3[1]][min_tmp_risk_list.Item3[2]].lgt, location[min_tmp_risk_list.Item3[0]][min_tmp_risk_list.Item3[1]][min_tmp_risk_list.Item3[2]].hgt};
                // float[] second = {location[target_om[0]][target_om[1]][target_om[2]].lat, location[target_om[0]][target_om[1]][target_om[2]].lgt, location[target_om[0]][target_om[1]][target_om[2]].hgt};
                                    
                float risk_ori = path_risk(start_om, target_om, location, risk_map);

                Console.WriteLine(risk_ori - min_risk);
                // Console.WriteLine(min_tmp_risk_list.Item1);

                if (level <= level_limit && (risk_ori - min_risk) > penalty && min_tmp_risk_list.Item1.Count == 3){
                    Console.WriteLine(risk_ori - min_risk);
                    if (true)
                    {
                        // 刪除問題待修正
                        Console.WriteLine((min_tmp_risk_list.Item3[0], min_tmp_risk_list.Item3[1]));
                        // Console.WriteLine(waypoint_local[min_index].Count);
                        // Program.waypoint_global[min_index].Add(1);
                        Program.waypoint_global.delete(min_index);
                        // Console.WriteLine(Program.waypoint_global.get(min_index).Count);
                        // Console.WriteLine(waypoint_local[min_index].Count);
                    }

                    List<List<int>> first_route = routing(start, min_tmp_risk_list.Item1, z, level, method, size, waypoint_local, 1, location, risk_map, map_init_location, margin_lat, margin_lng, level_limit, penalty);
                    List<List<int>> second_route = routing(min_tmp_risk_list.Item1, target, z, level, method, size, waypoint_local, 1, location, risk_map, map_init_location, margin_lat, margin_lng, level_limit, penalty);
                    // List<List<int>> intermediate_point = new List<List<int>> {min_tmp_risk_list.Item3};

                    first_route.Add(min_tmp_risk_list.Item3);
                    first_route.AddRange(second_route);

                    return first_route;
                }else{
                    return new List<List<int>> {new List<int>() {}};
                }
            }
        }
     }
}






