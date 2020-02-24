using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaCalc
{
    class GovernmentParameter
    {
        public int bed_count = 20000;

    }

    class CoronaParameter
    {
        public double R0_min;
        public double R0_max;
        public double org_death_rate;
        public double death_rate;
        public double death_rate_no_bed;
        public double serious_rate;
        public double no_symptoms_rate;
        public double no_symptoms_period;
        public double checkup_rate;
        public double checkup_symptoms_threshold;
        public int left_bed_count;
        public int org_virus_id;
        public int max_contact_number;

        public CoronaParameter()
        {
            R0_min = 3.9;
            R0_max = 6.6;
            death_rate = 0.05;
            death_rate_no_bed = 0.11;
            org_death_rate = 0.05;
            serious_rate = 0.25;
            no_symptoms_rate = 0.5;
            no_symptoms_period = 14;
            checkup_rate = 0.05;
            checkup_symptoms_threshold = 0.8;
            left_bed_count = 20000;
            max_contact_number = 70;

        }

    }

    class CoronaPatient
    {
        public int id;
        public int family_id;
        public int source_id;
        public int family_number;
        public int age;
        public int gender;
        public int use_mask;
        public int sick_period;
        public int checkup;
        public int checkup_relate;
        public int infected_number;
        public int contact_number;
        public int day;
        public int care_level;
        public int day_to_dead;
        public int dead;
        public int complete_cure;
        public double activitiy_factor;
        public double train_fator;
        public double room_factor;
        public double health_factor;
        public double serious_factor;

        public CoronaPatient(Random r, CoronaParameter c)
        {
            id = 0;
            family_id = 0;
            source_id = 0;
            checkup = 0;
            infected_number = 0;
            day = 0;
            day_to_dead = 0;
            dead = 0;
            complete_cure = 0;
            age = r.Next(100);
            gender = r.Next(2);
            family_number = r.Next(1, 6);
            care_level = r.Next(1, 6); //1 =何もしない 2=クリニック 3=呼吸器内科 4=入院 5=確診入院
            checkup_relate = r.Next(2);
            use_mask = r.Next(2); // 0=未着用 1=着用
            sick_period = r.Next(7, 14);
            contact_number = r.Next(0, c.max_contact_number);
            activitiy_factor = r.NextDouble();
            train_fator = r.NextDouble();
            room_factor = r.NextDouble();
            serious_factor = r.NextDouble();
            health_factor = r.NextDouble();

            double death_rate_int = (int)(c.death_rate * 100);
            int destiny_dice = r.Next(0, 100);

            if (dice(r, c.death_rate) == 1)
                day_to_dead = sick_period;
        }

        public int dice(Random r, double hit_rate)
        {
            hit_rate = (int)(hit_rate * 100);
            int destiny_dice = r.Next(0, 100);

            if (destiny_dice <= hit_rate)
                return 1;
            else
                return 0;
        }

        public List<CoronaPatient> infect(Random r, CoronaParameter c)
        {
            List<CoronaPatient> infect_list = new List<CoronaPatient>();

            if (this.care_level < 4)
            {
                for (int i = 0; i < contact_number; i++)
                {

                    CoronaPatient contact_p = new CoronaPatient(r, c);
                    if(contact_p.health_factor < 0.5) { contact_p.health_factor += 0.5; }
                    contact_p.care_level = 1; //感染されたばかりではなにもしない
                    contact_p.serious_factor = 0.1; //発症したばかりではひどくない

                    double infect_rate = activitiy_factor * train_fator * room_factor * serious_factor * (1.0 - use_mask) * (1.0 - contact_p.health_factor) * (1.0 - contact_p.use_mask);

                    if (dice(r, infect_rate) == 1)
                    {
                        infected_number++;
                        infect_list.Add(contact_p);
                    }
;

                }

                for (int i = 0; i < family_number; i++)
                {
                    CoronaPatient family_p = new CoronaPatient(r, c);
                    family_p.family_id = id;
                    family_p.source_id = id;
                    family_p.care_level = 1; //感染されたばかりではなにもしない
                    family_p.serious_factor = 0.1; //発症したばかりではひどくない
                    if (family_p.health_factor < 0.5) { family_p.health_factor += 0.5; }

                    double family_infect_rate = room_factor * 2 * serious_factor * (1.0 - family_p.health_factor);

                    if (dice(r, family_infect_rate) == 1)
                    {
                        infected_number++;
                        infect_list.Add(family_p);

                    }
                }
            }

            this.day++;

            return infect_list;
        }

        public int sick_step(Random r, CoronaParameter c)
        {

            if (this.dead == 0 && day_to_dead > 0)
            {

                double care_factor = 1.0 / this.care_level; //無視すれば一番進む
                double age_factor = (age * age) / 10000.0; //年寄り程危ない、指数的に
                double health_factor_2 = (health_factor - 0.5) * -0.2;
                double sick_step_factor = (age_factor * care_factor) + health_factor_2 + ((r.NextDouble() - 0.5) * 0.1);
                this.serious_factor += sick_step_factor;


                //1 =何もしない 2=クリニック 3=呼吸器内科 4=入院 5=確診入院
                if (serious_factor > 0.8 && c.left_bed_count > 0)
                {
                    care_level = 5;
                    serious_factor -= 0.1;
                }
                else if (serious_factor > 0.6 && serious_factor <= 0.8 && care_level < 4 && c.left_bed_count > 0)
                {
                    care_level = 4;
                    
                }
                else if (serious_factor > 0.4 && serious_factor <= 0.6 && care_level < 3)
                {
                    care_level = 3;
                    use_mask = 1;
                    contact_number = (int)(contact_number * 0.4);
                    activitiy_factor = activitiy_factor * 0.4;
                }
                else if (serious_factor > 0.2 && serious_factor <= 0.4)
                {
                    care_level = 2;
                    use_mask = dice(r, 0.6); //クリニックにいったら6割はマスク着用
                    contact_number = (int)(contact_number * 0.6);
                    activitiy_factor = activitiy_factor * 0.6;
                }

                if (day >= day_to_dead)
                    dead = 1;
            }

            return dead;

            
        }

        public int complete_cure_step()
        {
            if(dead == 0 && day_to_dead == 0)
            {
                if (day >= sick_period)
                    complete_cure = 1;
            }

            return complete_cure;
        }
    }
}
